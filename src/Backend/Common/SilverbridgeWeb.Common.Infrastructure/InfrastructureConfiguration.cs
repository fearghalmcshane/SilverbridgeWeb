using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Infrastructure.Clock;
using SilverbridgeWeb.Common.Infrastructure.Data;

namespace SilverbridgeWeb.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databaseConnectionString)
    {
        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
