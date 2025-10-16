using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SilverbridgeWeb.Common.Infrastructure.Authentication;

internal static class AuthenticationExtensions
{
    internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, string authConnectionString)
    {
        services.AddAuthorization();

        // Get the Keycloak URL from Aspire service discovery
        string keycloakUrl = authConnectionString ?? "http://localhost:8080";

        string realmUrl = $"{keycloakUrl}/realms/silverbridge";

        services.AddAuthentication().AddKeycloakJwtBearer("silverbridgeweb-auth", "silverbridge", options =>
        {
            options.Audience = "account";
            options.TokenValidationParameters = new()
            {
                ValidIssuers =
                [
                    realmUrl,
                    "http://localhost:8080/realms/silverbridge"
                ]
            };
            options.MetadataAddress = $"{realmUrl}/.well-known/openid-configuration";
            options.RequireHttpsMetadata = false;
        });

        services.AddHttpContextAccessor();

        return services;
    }
}
