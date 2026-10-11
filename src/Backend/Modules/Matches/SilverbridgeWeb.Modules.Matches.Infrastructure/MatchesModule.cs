using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Matches.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Database;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Matches;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Matches.Presentation;

namespace SilverbridgeWeb.Modules.Matches.Infrastructure;

public static class MatchesModule
{
    public static IServiceCollection AddMatchesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        services.AddInfrastructure(configuration);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);
        services.AddOptions<TeamTallyWebhookOptions>()
            .Bind(configuration.GetSection("TeamTally"))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.WebhookSigningSecret),
                "TeamTally:WebhookSigningSecret is required.")
            .ValidateOnStart();

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("silverbridgeDb")!;
        services.AddDbContext<MatchesDbContext>((sp, options) =>
            options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Database.Schemas.Matches))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MatchesDbContext>());
        services.AddScoped<IMatchCompletionRepository, MatchCompletionRepository>();
        services.Configure<MatchesOutboxOptions>(configuration.GetSection("Matches:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();
    }

    private static void AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] handlers = Application.AssemblyReference.Assembly.GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToArray();
        foreach (Type handler in handlers)
        {
            services.TryAddScoped(handler);
            Type domainEvent = handler.GetInterfaces()
                .Single(@interface => @interface.IsGenericType)
                .GetGenericArguments()
                .Single();
            services.Decorate(handler, typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent));
        }
    }
}
