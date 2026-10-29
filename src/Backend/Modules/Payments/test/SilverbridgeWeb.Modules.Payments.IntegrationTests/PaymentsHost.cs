using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Quartz;
using SilverbridgeWeb.Common.Application;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Infrastructure.Eventbus;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Database;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Inbox;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;
using EventUtility = Stripe.EventUtility;
using Testcontainers.PostgreSql;

namespace SilverbridgeWeb.Modules.Payments.IntegrationTests;

/// <summary>
/// The Payments module running against a real PostgreSQL database, with only the outbound Stripe API calls faked.
/// Webhook signatures, the webhook inbox and the outbox all run for real; background jobs are run on demand.
/// </summary>
[SuppressMessage("Design", "CA1515", Justification = "xUnit class fixtures must be public.")]
public sealed class PaymentsHost : IAsyncLifetime
{
    public const string WebhookSecret = "whsec_integration";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private WebApplication? _app;

    internal FakePaymentProvider Provider { get; } = new();

    internal RecordingEventBus EventBus { get; } = new();

    internal HttpClient Client { get; private set; } = null!;

    private WebApplication App => _app ?? throw new InvalidOperationException("The host has not started.");

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:silverbridgeDb"] = _postgres.GetConnectionString(),
            ["Payments:PublicBaseUrl"] = "https://club.test/",
            ["Payments:Stripe:SecretKey"] = "sk_test_unused",
            ["Payments:Stripe:WebhookSigningSecret"] = WebhookSecret,
            ["Payments:Webhooks:MaxAttempts"] = "3",
            ["Payments:Outbox:IntervalInSeconds"] = "60",
            ["Payments:Outbox:BatchSize"] = "100",
            ["Payments:Inbox:IntervalInSeconds"] = "60",
            ["Payments:Inbox:BatchSize"] = "100"
        });
        builder.Services.AddLogging();
        builder.Services.AddSingleton<InsertOutboxMessagesInterceptor>();
        builder.Services.AddSingleton<IDateTimeProvider, SystemClock>();
        builder.Services.AddSingleton<IDbConnectionFactory>(_ => new TestDbConnectionFactory(_postgres.GetConnectionString()));
        builder.Services.AddSingleton<IEventBus>(EventBus);
        builder.Services.AddApplication([Application.AssemblyReference.Assembly]);
        builder.Services.AddPaymentsModule(builder.Configuration);
        builder.Services.Replace(ServiceDescriptor.Singleton<IPaymentProvider>(Provider));

        _app = builder.Build();
        _app.MapEndpoints();

        await using (AsyncServiceScope scope = _app.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().Database.MigrateAsync();
        }

        await _app.StartAsync();
        string address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        Client = new HttpClient { BaseAddress = new Uri(address) };
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    internal async Task<Result> SendAsync(ICommand command)
    {
        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, TestContext.Current.CancellationToken);
    }

    internal async Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command)
    {
        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, TestContext.Current.CancellationToken);
    }

    internal async Task ConsumeAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent)
        where TIntegrationEvent : IntegrationEvent
    {
        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        await scope.ServiceProvider
            .GetRequiredService<IIntegrationEventConsumer<TIntegrationEvent>>()
            .ConsumeAsync(integrationEvent, TestContext.Current.CancellationToken);
    }

    internal Task RunInboxAsync() => RunJobAsync<ProcessInboxJob>();

    internal Task RunOutboxAsync() => RunJobAsync<ProcessOutboxJob>();

    internal Task RunWebhooksAsync() => RunJobAsync<ProcessProviderWebhooksJob>();

    internal async Task<HttpResponseMessage> PostWebhookAsync(string payload, string? signature = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/payments/webhooks/stripe")
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(payload))
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Headers.Add(
            "Stripe-Signature",
            signature ?? EventUtility.GenerateSignatureHeader(payload, WebhookSecret, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

        return await Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    internal async Task<PaymentRequest> GetPaymentRequestAsync(Guid id)
    {
        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().PaymentRequests
            .AsNoTracking()
            .Include(p => p.Lines)
            .Include(p => p.Refunds)
            .SingleAsync(p => p.Id == id, TestContext.Current.CancellationToken);
    }

    internal async Task<Subscription?> GetSubscriptionAsync(Guid id)
    {
        await using AsyncServiceScope scope = App.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().Subscriptions
            .AsNoTracking()
            .Include(s => s.Payments)
            .SingleOrDefaultAsync(s => s.Id == id, TestContext.Current.CancellationToken);
    }

    internal async Task<T> QueryScalarAsync<T>(string sql, object parameters)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        return await Dapper.SqlMapper.ExecuteScalarAsync<T>(connection, sql, parameters) ??
               throw new InvalidOperationException("The query returned nothing.");
    }

    internal async Task ExecuteAsync(string sql, object parameters)
    {
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await Dapper.SqlMapper.ExecuteAsync(connection, sql, parameters);
    }

    private async Task RunJobAsync<TJob>()
        where TJob : IJob
    {
        TJob job = ActivatorUtilities.CreateInstance<TJob>(App.Services);
        IJobExecutionContext context = DispatchProxy.Create<IJobExecutionContext, TestJobExecutionContext>();
        ((TestJobExecutionContext)(object)context).CancellationToken = TestContext.Current.CancellationToken;
        await job.Execute(context);
    }

    private sealed class SystemClock : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class TestDbConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public async ValueTask<DbConnection> OpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }

    [SuppressMessage("Design", "CA1515", Justification = "DispatchProxy emits a derived proxy type in a separate assembly.")]
    public class TestJobExecutionContext : DispatchProxy
    {
        public CancellationToken CancellationToken { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == "get_CancellationToken")
            {
                return CancellationToken;
            }

            throw new NotSupportedException($"Unexpected Quartz job context member: {targetMethod?.Name}");
        }
    }
}
