using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Common.Infrastructure.Interceptors;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Identity;
using SilverbridgeWeb.Modules.Users.Domain.Users;
using SilverbridgeWeb.Modules.Users.Infrastructure.Database;
using SilverbridgeWeb.Modules.Users.Infrastructure.Identity;
using SilverbridgeWeb.Modules.Users.Infrastructure.Users;

namespace SilverbridgeWeb.Modules.Users.Infrastructure;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("silverbridgeDb")!;

        string keycloakUrl = configuration["KeyCloak:Url"]!;

        services.AddOptions<KeyCloakOptions>()
            .Configure(options =>
            {
                string realm = configuration["KeyCloak:Realm"] ?? "silverbridge";

                options.AdminUrl = $"{keycloakUrl}/admin/realms/{realm}/";
                options.TokenUrl = $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token";
                options.ConfidentialClientId = configuration["KeyCloak:ConfidentialClientId"]!;
                options.ConfidentialClientSecret = configuration["KeyCloak:ConfidentialClientSecret"]!;
                options.PublicClientId = configuration["KeyCloak:PublicClientId"]!;
            });

        services.AddTransient<KeyCloakAuthDelegatingHandler>();

        services
            .AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
            {
                KeyCloakOptions keyCloakOptions = serviceProvider
                    .GetRequiredService<IOptions<KeyCloakOptions>>().Value;

                httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
            })
            .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

        services.AddTransient<IIdentityProviderService, IdentityProviderService>();

        services.AddDbContext<UsersDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString(databaseConnectionString),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());
    }
}
