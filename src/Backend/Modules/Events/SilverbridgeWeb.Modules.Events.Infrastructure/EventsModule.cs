using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Events.Domain.Categories;
using SilverbridgeWeb.Modules.Events.Domain.Events;
using SilverbridgeWeb.Modules.Events.Domain.TicketTypes;
using SilverbridgeWeb.Modules.Events.Infrastructure.Categories;
using SilverbridgeWeb.Modules.Events.Infrastructure.Database;
using SilverbridgeWeb.Modules.Events.Infrastructure.Events;
using SilverbridgeWeb.Modules.Events.Infrastructure.TicketTypes;
using SilverbridgeWeb.Modules.Events.Presentation.Categories;
using SilverbridgeWeb.Modules.Events.Presentation.Events;
using SilverbridgeWeb.Modules.Events.Presentation.TicketTypes;

namespace SilverbridgeWeb.Modules.Events.Infrastructure;

public static class EventsModule
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        TicketTypeEndpoints.MapEndpoints(app);
        CategoryEndpoints.MapEndpoints(app);
        EventEndpoints.MapEndpoints(app);
    }

    public static TBuilder AddEventsModule<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        services.AddDbContext<EventsDbContext>(options =>
            options.UseNpgsql(databaseConnectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Events)).UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EventsDbContext>());

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
    }
}
