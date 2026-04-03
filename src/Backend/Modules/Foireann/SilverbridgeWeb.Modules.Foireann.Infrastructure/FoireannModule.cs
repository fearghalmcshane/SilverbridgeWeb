using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Foireann.Application.Abstractions;

namespace SilverbridgeWeb.Modules.Foireann.Infrastructure;

public static class FoireannModule
{
    public static IServiceCollection AddFoireannModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FoireannOptions>(configuration.GetSection("Foireann"));

        services.AddTransient<FoireannAuthDelegatingHandler>();

        services
            .AddHttpClient<FoireannClient>((serviceProvider, httpClient) =>
            {
                FoireannOptions options = serviceProvider
                    .GetRequiredService<IOptions<FoireannOptions>>().Value;

                httpClient.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<FoireannAuthDelegatingHandler>();

        services.AddScoped<IFoireannService, CachedFoireannService>();
    }
}
