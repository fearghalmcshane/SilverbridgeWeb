using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace SilverbridgeWeb.Common.Infrastructure.Authentication;

internal static class AuthenticationExtensions
{
    internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, string clerkAuthority)
    {
        services.AddAuthorizationBuilder();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = clerkAuthority;
#pragma warning disable CA5404 // Clerk tokens do not include an audience claim by default; configure ValidAudiences once known
                options.TokenValidationParameters = new()
                {
                    ValidateAudience = false
                };
#pragma warning restore CA5404
                options.RequireHttpsMetadata = false;
            });

        services.AddHttpContextAccessor();

        return services;
    }
}
