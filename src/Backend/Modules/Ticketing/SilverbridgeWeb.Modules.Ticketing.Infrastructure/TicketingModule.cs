using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Ticketing.Application.Carts;
using SilverbridgeWeb.Modules.Ticketing.Domain.Customers;
using SilverbridgeWeb.Modules.Ticketing.Domain.Events;
using SilverbridgeWeb.Modules.Ticketing.Domain.Orders;
using SilverbridgeWeb.Modules.Ticketing.Domain.Payments;
using SilverbridgeWeb.Modules.Ticketing.Domain.Tickets;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Authentication;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Customers;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Database;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Events;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Orders;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Payments;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Tickets;
using SilverbridgeWeb.Modules.Ticketing.Presentation.Customers;
using SilverbridgeWeb.Modules.Ticketing.Presentation.Events;
using SilverbridgeWeb.Modules.Ticketing.Presentation.TicketTypes;

namespace SilverbridgeWeb.Modules.Ticketing.Infrastructure;

public static class TicketingModule
{
    public static IServiceCollection AddTicketingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        registrationConfigurator.AddConsumer<UserRegisteredIntegrationEventConsumer>();
        registrationConfigurator.AddConsumer<UserProfileUpdatedIntegrationEventConsumer>();
        registrationConfigurator.AddConsumer<EventPublishedIntegrationEventConsumer>();
        registrationConfigurator.AddConsumer<TicketTypePriceChangedIntegrationEventConsumer>();
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        services.AddDbContext<TicketingDbContext>((sp, options) =>
            options
                .UseNpgsql(databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Ticketing))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TicketingDbContext>());

        services.AddSingleton<CartService>();
        services.AddSingleton<IPaymentService, PaymentService>();

        services.AddScoped<ICustomerContext, CustomerContext>();

        services.Configure<OutboxOptions>(configuration.GetSection("Ticketing:Outbox"));

        services.ConfigureOptions<ConfigureProcessOutboxJob>();
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
}
