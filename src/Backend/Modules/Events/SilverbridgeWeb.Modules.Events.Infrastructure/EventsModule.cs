using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.Common.Infrastructure.Interceptors;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Events.Domain.Categories;
using SilverbridgeWeb.Modules.Events.Domain.Events;
using SilverbridgeWeb.Modules.Events.Domain.TicketTypes;
using SilverbridgeWeb.Modules.Events.Infrastructure.Categories;
using SilverbridgeWeb.Modules.Events.Infrastructure.Database;
using SilverbridgeWeb.Modules.Events.Infrastructure.Events;
using SilverbridgeWeb.Modules.Events.Infrastructure.TicketTypes;

namespace SilverbridgeWeb.Modules.Events.Infrastructure;

public static class EventsModule
{
    public static IServiceCollection AddEventsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        services.AddInfrastructure(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        services.AddDbContext<EventsDbContext>((sp, options) =>
            options.UseNpgsql(databaseConnectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Events))
            .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>())
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EventsDbContext>());

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
    }
}
