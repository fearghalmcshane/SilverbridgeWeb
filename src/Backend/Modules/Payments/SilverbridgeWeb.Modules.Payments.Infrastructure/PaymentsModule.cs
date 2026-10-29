using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Infrastructure.Eventbus;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Authentication;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Database;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Inbox;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Payers;
using SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Reconciliation;
using SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;
using SilverbridgeWeb.Modules.Users.IntegrationEvents;
using Stripe;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();

        services.AddIntegrationEventHandlers();

        services.AddConsumers();

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddConsumers(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer<UserRegisteredIntegrationEvent>, IntegrationEventConsumer<UserRegisteredIntegrationEvent>>();
        services.AddScoped<IIntegrationEventConsumer<UserProfileUpdatedIntegrationEvent>, IntegrationEventConsumer<UserProfileUpdatedIntegrationEvent>>();
        services.AddScoped<IIntegrationEventConsumer<PaymentRequestedIntegrationEvent>, IntegrationEventConsumer<PaymentRequestedIntegrationEvent>>();
        services.AddScoped<IIntegrationEventConsumer<PaymentRequestCancelledIntegrationEvent>, IntegrationEventConsumer<PaymentRequestCancelledIntegrationEvent>>();
        services.AddScoped<IIntegrationEventConsumer<RefundRequestedIntegrationEvent>, IntegrationEventConsumer<RefundRequestedIntegrationEvent>>();
        services.AddScoped<IIntegrationEventConsumer<SubscriptionCancellationRequestedIntegrationEvent>, IntegrationEventConsumer<SubscriptionCancellationRequestedIntegrationEvent>>();
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        services.AddDbContext<PaymentsDbContext>((sp, options) =>
            options
                .UseNpgsql(databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Payments))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IPaymentRequestRepository, PaymentRequestRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPayerRepository, PayerRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<IPaymentsUserContext, PaymentsUserContext>();

        services.AddOptions<PaymentsOptions>()
            .Bind(configuration.GetSection(PaymentsOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Stripe.SecretKey),
                "Payments:Stripe:SecretKey is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Stripe.WebhookSigningSecret),
                "Payments:Stripe:WebhookSigningSecret is required.")
            .Validate(
                options => options.PublicBaseUrl is { IsAbsoluteUri: true },
                "Payments:PublicBaseUrl must be an absolute URL.")
            .ValidateOnStart();

        services.TryAddSingleton(sp => new StripeClient(
            new StripeClientOptions { ApiKey = sp.GetRequiredService<IOptions<PaymentsOptions>>().Value.Stripe.SecretKey }));
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddSingleton<IProviderWebhookVerifier, StripeWebhookVerifier>();
        services.AddScoped<IProviderWebhookInbox, ProviderWebhookInbox>();

        services.ConfigureOptions<ConfigureProcessProviderWebhooksJob>();
        services.ConfigureOptions<ConfigureReconcilePendingPaymentsJob>();

        services.Configure<OutboxOptions>(configuration.GetSection("Payments:Outbox"));

        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        services.Configure<InboxOptions>(configuration.GetSection("Payments:Inbox"));

        services.ConfigureOptions<ConfigureProcessInboxJob>();
    }

    private static void AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] domainEventHandlers = Application.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToArray();

        foreach (Type domainEventHandler in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandler);

            Type domainEvent = domainEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

            services.Decorate(domainEventHandler, closedIdempotentHandler);
        }
    }

    private static void AddIntegrationEventHandlers(this IServiceCollection services)
    {
        Type[] integrationEventHandlers = Presentation.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToArray();

        foreach (Type integrationEventHandler in integrationEventHandlers)
        {
            services.TryAddScoped(integrationEventHandler);

            Type integrationEvent = integrationEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler =
                typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

            services.Decorate(integrationEventHandler, closedIdempotentHandler);
        }
    }
}
