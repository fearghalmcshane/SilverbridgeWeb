using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.Common.Infrastructure.Interceptors;
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
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Payments;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Tickets;
using SilverbridgeWeb.Modules.Ticketing.Presentation.Customers;

namespace SilverbridgeWeb.Modules.Ticketing.Infrastructure;

public static class TicketingModule
{
    public static IServiceCollection AddTicketingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        registrationConfigurator.AddConsumer<UserRegisteredIntegrationEventConsumer>();
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        services.AddDbContext<TicketingDbContext>((sp, options) =>
            options
                .UseNpgsql(databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Ticketing))
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>())
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
    }
}
