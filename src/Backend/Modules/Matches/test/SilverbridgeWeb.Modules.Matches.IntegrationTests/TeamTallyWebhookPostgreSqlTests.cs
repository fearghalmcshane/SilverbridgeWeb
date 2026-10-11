using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.Common.Application;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Infrastructure;
using SilverbridgeWeb.Common.Infrastructure.Eventbus;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Matches.Application;
using SilverbridgeWeb.Modules.Matches.Infrastructure;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Database;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Matches;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Outbox;
using SilverbridgeWeb.WebUI.Services.Webhooks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Quartz;
using Testcontainers.PostgreSql;
using OutboxMessage = SilverbridgeWeb.Common.Infrastructure.Outbox.OutboxMessage;

namespace SilverbridgeWeb.Modules.Matches.IntegrationTests;

public sealed class TeamTallyWebhookPostgreSqlTests
{
    private const string Secret = "integration-raw-secret";

    [Fact]
    public async Task SignedWebhook_PersistsCompletionAndOutboxOnce_AndConflictsOnReusedEventId()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await using PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync(cancellationToken);

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:silverbridgeDb"] = postgres.GetConnectionString(),
            ["TeamTally:WebhookSigningSecret"] = Secret,
            ["Matches:Outbox:IntervalInSeconds"] = "60",
            ["Matches:Outbox:BatchSize"] = "20"
        });
        builder.Services.AddLogging();
        builder.Services.AddSingleton<InsertOutboxMessagesInterceptor>();
        builder.Services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();
        builder.Services.AddSingleton<IDbConnectionFactory>(_ => new TestDbConnectionFactory(postgres.GetConnectionString()));
        var eventBus = new RecordingEventBus();
        builder.Services.AddSingleton<IEventBus>(eventBus);
        builder.Services.AddApplication([SilverbridgeWeb.Modules.Matches.Application.AssemblyReference.Assembly]);
        builder.Services.AddMatchesModule(builder.Configuration);

        await using WebApplication app = builder.Build();
        app.MapEndpoints();
        await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            MatchesDbContext database = scope.ServiceProvider.GetRequiredService<MatchesDbContext>();
            await database.Database.MigrateAsync(cancellationToken);
        }

        await app.StartAsync(cancellationToken);
        IServer server = app.Services.GetRequiredService<IServer>();
        string address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var backendClient = new HttpClient { BaseAddress = new Uri(address) };
        WebApplicationBuilder relayBuilder = WebApplication.CreateBuilder();
        relayBuilder.WebHost.UseUrls("http://127.0.0.1:0");
        relayBuilder.Services.AddSingleton(new TeamTallyWebhookRelay(backendClient));
        await using WebApplication relayApp = relayBuilder.Build();
        TeamTallyWebhookRelay.MapEndpoint(relayApp);
        await relayApp.StartAsync(cancellationToken);
        IServer relayServer = relayApp.Services.GetRequiredService<IServer>();
        string relayAddress = relayServer.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient { BaseAddress = new Uri(relayAddress) };

        using HttpResponseMessage relayOversizedResponse = await SendOversizedAsync(
            client,
            "/webhooks/team-tally",
            cancellationToken);
        using HttpResponseMessage apiOversizedResponse = await SendOversizedAsync(
            backendClient,
            "/matches/webhooks/team-tally",
            cancellationToken);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, relayOversizedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, apiOversizedResponse.StatusCode);

        string firstPayload = CreatePayload("evt-1", "match/opaque", "2-10");
        using HttpResponseMessage firstResponse = await SendAsync(client, firstPayload, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        string duplicatePayload = CreatePayload("evt-2", "match/opaque", "8-1");
        using HttpResponseMessage duplicateResponse = await SendAsync(client, duplicatePayload, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, duplicateResponse.StatusCode);

        string conflictingPayload = CreatePayload("evt-1", "match/other", "1-1");
        using HttpResponseMessage conflictResponse = await SendAsync(client, conflictingPayload, cancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        HttpResponseMessage[] concurrentResponses = await Task.WhenAll(
            SendAsync(client, CreatePayload("evt-3", "match/concurrent", "2-10"), cancellationToken),
            SendAsync(client, CreatePayload("evt-4", "match/concurrent", "2-10"), cancellationToken));
        foreach (HttpResponseMessage response in concurrentResponses)
        {
            using (response)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        using HttpResponseMessage existingMatchConflict = await SendAsync(
            client, CreatePayload("evt-1", "match/concurrent", "2-10"), cancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, existingMatchConflict.StatusCode);

        string oversizedNamePayload = CreatePayload("evt-too-long", "match/too-long", "2-10")
            .Replace("Cúil Óg", new string('x', 301), StringComparison.Ordinal);
        using HttpResponseMessage invalidLengthResponse = await SendAsync(client, oversizedNamePayload, cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, invalidLengthResponse.StatusCode);

        string mixedCasePayload = CreatePayload("evt-status", "match/opaque", "2-10")
            .Replace("\"completed\"", "\"Completed\"", StringComparison.Ordinal);
        using HttpResponseMessage mixedCaseResponse = await SendAsync(client, mixedCasePayload, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, mixedCaseResponse.StatusCode);

        await using AsyncServiceScope verifyScope = app.Services.CreateAsyncScope();
        MatchesDbContext verifyDatabase = verifyScope.ServiceProvider.GetRequiredService<MatchesDbContext>();
        MatchCompletion storedCompletion = await verifyDatabase.Set<MatchCompletion>()
            .SingleAsync(completion => completion.ProviderMatchId == "match/opaque", cancellationToken);
        Assert.Equal("evt-1", storedCompletion.ProviderEventId);
        Assert.Equal("2-10", storedCompletion.TeamScoreDisplay);
        Assert.Equal(firstPayload, storedCompletion.OriginalJson);
        Assert.Equal(2, await verifyDatabase.Set<MatchCompletion>().CountAsync(cancellationToken));
        Assert.Equal(2, await verifyDatabase.Set<OutboxMessage>().CountAsync(cancellationToken));

        Guid replayMessageId = await verifyDatabase.Set<OutboxMessage>()
            .Select(message => message.Id)
            .FirstAsync(cancellationToken);
        var outboxJob = new ProcessOutboxJob(
            app.Services.GetRequiredService<IDbConnectionFactory>(),
            app.Services.GetRequiredService<IServiceScopeFactory>(),
            app.Services.GetRequiredService<IDateTimeProvider>(),
            Options.Create(new MatchesOutboxOptions { BatchSize = 20 }),
            app.Services.GetRequiredService<ILogger<ProcessOutboxJob>>());
        IJobExecutionContext jobContext = DispatchProxy.Create<IJobExecutionContext, TestJobExecutionContext>();
        ((TestJobExecutionContext)(object)jobContext).CancellationToken = cancellationToken;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            await outboxJob.Execute(jobContext);
            await verifyDatabase.Database.ExecuteSqlRawAsync(
                "UPDATE matches.outbox_messages SET next_attempt_on_utc = now() - interval '1 second' WHERE processed_on_utc IS NULL AND exhausted_on_utc IS NULL",
                cancellationToken);
        }

        OutboxMessage exhaustedMessage = await verifyDatabase.Set<OutboxMessage>()
            .AsNoTracking()
            .SingleAsync(message => message.Id == replayMessageId, cancellationToken);
        Assert.Null(exhaustedMessage.ProcessedOnUtc);
        Assert.NotNull(exhaustedMessage.Error);
        Assert.Contains("integration event publication failed", exhaustedMessage.Error, StringComparison.Ordinal);
        Assert.Equal(5, await verifyDatabase.Database
            .SqlQuery<int>($"SELECT attempt_count AS \"Value\" FROM matches.outbox_messages WHERE id = {replayMessageId}")
            .SingleAsync(cancellationToken));

        await verifyDatabase.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE matches.outbox_messages SET attempt_count = 0, next_attempt_on_utc = NULL, exhausted_on_utc = NULL, error = NULL WHERE id = {replayMessageId}",
            cancellationToken);
        eventBus.ShouldFail = false;
        await outboxJob.Execute(jobContext);
        exhaustedMessage = await verifyDatabase.Set<OutboxMessage>()
            .AsNoTracking()
            .SingleAsync(message => message.Id == replayMessageId, cancellationToken);
        Assert.NotNull(exhaustedMessage.ProcessedOnUtc);
        Assert.Equal(6, eventBus.PublishedEventIds.Count(id => id == replayMessageId));

        Guid cancellationMessageId = await verifyDatabase.Set<OutboxMessage>()
            .Where(message => message.Id != replayMessageId)
            .Select(message => message.Id)
            .SingleAsync(cancellationToken);
        DateTime scheduledRetry = DateTime.UtcNow.AddSeconds(-1);
        scheduledRetry = scheduledRetry.AddTicks(-(scheduledRetry.Ticks % 10));
        const string preCancellationError = "prior retry error";
        await verifyDatabase.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE matches.outbox_messages SET attempt_count = 2, next_attempt_on_utc = {scheduledRetry}, exhausted_on_utc = NULL, error = {preCancellationError}, processed_on_utc = NULL WHERE id = {cancellationMessageId}",
            cancellationToken);

        using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        eventBus.CancelOnPublish = cancellationSource;
        IJobExecutionContext cancelledJobContext =
            DispatchProxy.Create<IJobExecutionContext, TestJobExecutionContext>();
        ((TestJobExecutionContext)(object)cancelledJobContext).CancellationToken = cancellationSource.Token;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => outboxJob.Execute(cancelledJobContext));
        OutboxRetryState cancelledState = await ReadOutboxStateAsync(
            postgres.GetConnectionString(),
            cancellationMessageId,
            cancellationToken);
        Assert.Equal(2, cancelledState.AttemptCount);
        Assert.Equal(preCancellationError, cancelledState.Error);
        Assert.Null(cancelledState.ProcessedOnUtc);
        Assert.Null(cancelledState.ExhaustedOnUtc);
        Assert.Equal(scheduledRetry, cancelledState.NextAttemptOnUtc);
        eventBus.CancelOnPublish = null;

        await verifyDatabase.Database.ExecuteSqlRawAsync(
            """
            CREATE OR REPLACE FUNCTION matches.reject_outbox_insert()
            RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                RAISE EXCEPTION 'integration test outbox rejection';
            END;
            $$;
            CREATE TRIGGER reject_outbox_insert
            BEFORE INSERT ON matches.outbox_messages
            FOR EACH ROW EXECUTE FUNCTION matches.reject_outbox_insert();
            """,
            cancellationToken);
        using HttpResponseMessage failedSaveResponse = await SendAsync(
            client,
            CreatePayload("evt-5", "match/rollback", "0-0"),
            cancellationToken);
        Assert.Equal(HttpStatusCode.InternalServerError, failedSaveResponse.StatusCode);
        Assert.Equal(2, await verifyDatabase.Set<MatchCompletion>().CountAsync(cancellationToken));
        Assert.Equal(2, await verifyDatabase.Set<OutboxMessage>().CountAsync(cancellationToken));

        await relayApp.StopAsync(cancellationToken);
        await app.StopAsync(cancellationToken);
    }

    [Fact]
    public async Task Capture_ConcurrentMatchAndEventOwnersDoNotMaskConflict()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await using PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync(cancellationToken);
        DbContextOptions<MatchesDbContext> options = new DbContextOptionsBuilder<MatchesDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new InsertOutboxMessagesInterceptor())
            .Options;
        await using var winnerDatabase = new MatchesDbContext(options);
        await winnerDatabase.Database.MigrateAsync(cancellationToken);

        var concurrentInsert = new ConcurrentInsertInterceptor(async () =>
        {
            winnerDatabase.Add(CreateCompletion("evt-owned", "match/owner"));
            winnerDatabase.Add(CreateCompletion("evt-winner", "match/target"));
            await winnerDatabase.SaveChangesAsync(cancellationToken);
        });
        DbContextOptions<MatchesDbContext> contenderOptions = new DbContextOptionsBuilder<MatchesDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(concurrentInsert, new InsertOutboxMessagesInterceptor())
            .Options;
        await using var contenderDatabase = new MatchesDbContext(contenderOptions);
        var repository = new MatchCompletionRepository(contenderDatabase);

        MatchCompletionCaptureOutcome outcome = await repository.CaptureAsync(
            CreateCompletion("evt-owned", "match/target"), cancellationToken);

        Assert.True(concurrentInsert.WasInvoked);
        Assert.Equal(MatchCompletionCaptureOutcome.EventIdConflict, outcome);
        Assert.Equal(2, await winnerDatabase.Set<MatchCompletion>().CountAsync(cancellationToken));
        Assert.Equal(2, await winnerDatabase.Set<OutboxMessage>().CountAsync(cancellationToken));
        Assert.Empty(contenderDatabase.ChangeTracker.Entries());
    }

    private static MatchCompletion CreateCompletion(string eventId, string matchId)
    {
        string payload = CreatePayload(eventId, matchId, "2-10");
        SilverbridgeWeb.Modules.Matches.Presentation.ParsedTeamTallyEvent parsed =
            SilverbridgeWeb.Modules.Matches.Presentation.TeamTallyWebhookPayloadParser.Parse(payload, DateTime.UtcNow);
        return MatchCompletion.Create(parsed.Completion!, payload);
    }

    private sealed class ConcurrentInsertInterceptor(Func<Task> insertWinners) : SaveChangesInterceptor
    {
        public bool WasInvoked { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (!WasInvoked)
            {
                WasInvoked = true;
                await insertWinners();
            }

            return result;
        }
    }

    [Fact]
    public async Task DefaultEventBus_SucceedsWhenNoConsumersAreRegistered()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(
            "Host=localhost;Database=matches;Username=matches;Password=matches",
            "localhost:1",
            "https://clerk.example");
        using ServiceProvider provider = services.BuildServiceProvider();
        IEventBus eventBus = provider.GetRequiredService<IEventBus>();
        Assert.Empty(provider.GetServices<IIntegrationEventConsumer<
            SilverbridgeWeb.Modules.Matches.IntegrationEvents.MatchCompletedIntegrationEvent>>());
        var snapshot = new SilverbridgeWeb.Modules.Matches.IntegrationEvents.MatchCompletedSnapshot(
            "evt",
            "match",
            "match.completed",
            "v3",
            DateTime.UtcNow,
            DateTime.UtcNow,
            "completed",
            "hurling",
            null,
            null,
            DateTime.UtcNow,
            null,
            null,
            "Team",
            new SilverbridgeWeb.Modules.Matches.IntegrationEvents.MatchScore(1, 1, 4, "1-1"),
            "Opponent",
            new SilverbridgeWeb.Modules.Matches.IntegrationEvents.MatchScore(0, 0, 0, "0-0"),
            "win",
            null,
            null);

        await eventBus.PublishAsync(
            new SilverbridgeWeb.Modules.Matches.IntegrationEvents.MatchCompletedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                snapshot),
            TestContext.Current.CancellationToken);
    }

    private static async Task<OutboxRetryState> ReadOutboxStateAsync(
        string connectionString,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QuerySingleAsync<OutboxRetryState>(
            """
            SELECT attempt_count AS AttemptCount,
                   error AS Error,
                   processed_on_utc AS ProcessedOnUtc,
                   exhausted_on_utc AS ExhaustedOnUtc,
                   next_attempt_on_utc AS NextAttemptOnUtc
            FROM matches.outbox_messages
            WHERE id = @MessageId
            """,
            new { MessageId = messageId });
    }

    private static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        string payload,
        CancellationToken cancellationToken)
    {
        byte[] body = Encoding.UTF8.GetBytes(payload);
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        byte[] signedPayload = Encoding.UTF8.GetBytes(timestamp + ".").Concat(body).ToArray();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        string signature = Convert.ToBase64String(hmac.ComputeHash(signedPayload));
        using var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/team-tally")
        {
            Content = new ByteArrayContent(body)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Headers.Add("TeamTally-Signature", $"t={timestamp},v1={signature}");

        return await client.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> SendOversizedAsync(
        HttpClient client,
        string path,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new OversizedChunkedContent()
        };
        return await client.SendAsync(request, cancellationToken);
    }

    private static string CreatePayload(string eventId, string matchId, string display) =>
        """
        {"id":"__EVENT__","type":"match.completed","api_version":"v3","created_at":"2026-10-11T20:00:00Z",
         "data":{"id":"__MATCH__","status":"completed","sport":"hurling","ended_at":"2026-10-11T19:00:00Z",
         "team":{"name":"Cúil Óg","score":{"goals":2,"points":10,"total":16,"display":"__DISPLAY__"}},
         "opponent":{"name":"Áth na Rí","score":{"goals":1,"points":8,"total":11,"display":"1-8"}},
         "result":"win","summary":"Won"}}
        """
        .Replace("__EVENT__", eventId, StringComparison.Ordinal)
        .Replace("__MATCH__", matchId, StringComparison.Ordinal)
        .Replace("__DISPLAY__", display, StringComparison.Ordinal);

    private sealed class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class TestDbConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public async ValueTask<DbConnection> OpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            return connection;
        }
    }

    private sealed class RecordingEventBus : IEventBus
    {
        public List<Guid> PublishedEventIds { get; } = [];

        public bool ShouldFail { get; set; } = true;

        public CancellationTokenSource? CancelOnPublish { get; set; }

        public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
            where T : IIntegrationEvent
        {
            cancellationToken.ThrowIfCancellationRequested();
            PublishedEventIds.Add(integrationEvent.Id);
            if (CancelOnPublish is not null)
            {
                await CancelOnPublish.CancelAsync();
                throw new InvalidOperationException(
                    "event bus wrapped shutdown cancellation",
                    new OperationCanceledException(cancellationToken));
            }

            if (ShouldFail)
            {
                throw new InvalidOperationException("integration event publication failed");
            }

        }
    }

    private sealed record OutboxRetryState(
        int AttemptCount,
        string? Error,
        DateTime? ProcessedOnUtc,
        DateTime? ExhaustedOnUtc,
        DateTime? NextAttemptOnUtc);

    private sealed class OversizedChunkedContent : HttpContent
    {
        private readonly byte[] _body = new byte[1024 * 1024 + 1];

        protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context) =>
            stream.WriteAsync(_body).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
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
