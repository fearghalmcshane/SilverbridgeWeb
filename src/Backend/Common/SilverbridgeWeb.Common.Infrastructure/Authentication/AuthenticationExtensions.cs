using Microsoft.Extensions.DependencyInjection;

namespace SilverbridgeWeb.Common.Infrastructure.Authentication;

internal static class AuthenticationExtensions
{
    internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, string authConnectionString)
    {
        services.AddAuthorizationBuilder();

        services.AddAuthentication().AddKeycloakJwtBearer("silverbridgewebAuth", "silverbridge", options =>
        {
            options.Audience = "silverbridgeweb-api";
            options.TokenValidationParameters = new()
            {
                ValidIssuers =
                [
                    authConnectionString
                ]
            };
            options.MetadataAddress = $"{authConnectionString}/.well-known/openid-configuration";
            options.RequireHttpsMetadata = false;
        });

        services.AddHttpContextAccessor();

        return services;
    }
}
