using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SilverbridgeWeb.Modules.Events.Api.Database;
using SilverbridgeWeb.Modules.Events.Api.Events;

namespace SilverbridgeWeb.Modules.Events.Api;

public static class EventsModule
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        CreateEvent.MapEndpoint(app);
        GetEvent.MapEndpoint(app);
    }

    public static TBuilder AddEventsModule<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddDbContext<EventsDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("silverbridgeDb"), 
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Events)).UseSnakeCaseNamingConvention());

        return builder;
    }
}
