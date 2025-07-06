using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Events.Domain.Events;
using SilverbridgeWeb.Modules.Events.Infrastructure.Data;
using SilverbridgeWeb.Modules.Events.Infrastructure.Database;
using SilverbridgeWeb.Modules.Events.Infrastructure.Events;
using SilverbridgeWeb.Modules.Events.Presentation.Events;

namespace SilverbridgeWeb.Modules.Events.Infrastructure;

public static class EventsModule
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        EventEndpoints.MapEndpoints(app);
    }

    public static TBuilder AddEventsModule<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly);
        });

        builder.Services.AddValidatorsFromAssembly(Application.AssemblyReference.Assembly, includeInternalTypes: true);

        builder.Services.AddInfrastructure(builder.Configuration);

        return builder;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb");

        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.AddDbContext<EventsDbContext>(options =>
            options.UseNpgsql(databaseConnectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Events)).UseSnakeCaseNamingConvention());

        services.AddScoped<IEventRepository, EventRepository>();

        services.AddScoped<IUnitOfwork>(sp => sp.GetRequiredService<EventsDbContext>());
    }
}
