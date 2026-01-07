using Dapper;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Quartz;
using SilverbridgeWeb.Common.Application.Caching;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Infrastructure.Authentication;
using SilverbridgeWeb.Common.Infrastructure.Authorization;
using SilverbridgeWeb.Common.Infrastructure.Caching;
using SilverbridgeWeb.Common.Infrastructure.Clock;
using SilverbridgeWeb.Common.Infrastructure.Data;
using SilverbridgeWeb.Common.Infrastructure.Eventbus;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using StackExchange.Redis;

namespace SilverbridgeWeb.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<IRegistrationConfigurator>[] moduleConfigureconsumers,
        string databaseConnectionString,
        string redisConnectionString,
        string authConnectionString)
    {
        services.AddAuthenticationInternal(authConnectionString);

        services.AddAuthorizationInternal();

        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

        services.AddQuartz();

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.TryAddSingleton(connectionMultiplexer);

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
            });
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }

        services.TryAddSingleton<ICacheService, CacheService>();
        services.TryAddSingleton<IEventBus, EventBus>();

        services.AddMassTransit(configure =>
        {
            foreach (Action<IRegistrationConfigurator> configureConsumer in moduleConfigureconsumers)
            {
                configureConsumer(configure);
            }

            configure.SetKebabCaseEndpointNameFormatter();

            configure.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
