using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Events.Domain.Categories;
using SilverbridgeWeb.Modules.Events.Domain.Events;
using SilverbridgeWeb.Modules.Events.Domain.TicketTypes;
using SilverbridgeWeb.Modules.Events.Infrastructure.Categories;
using SilverbridgeWeb.Modules.Events.Infrastructure.Database;
using SilverbridgeWeb.Modules.Events.Infrastructure.Events;
using SilverbridgeWeb.Modules.Events.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Events.Infrastructure.TicketTypes;

namespace SilverbridgeWeb.Modules.Events.Infrastructure;

public static class EventsModule
{
    public static IServiceCollection AddEventsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();

        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        services.AddDbContext<EventsDbContext>((sp, options) =>
            options.UseNpgsql(databaseConnectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Events))
            .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EventsDbContext>());

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.Configure<OutboxOptions>(configuration.GetSection("Events:Outbox"));

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
