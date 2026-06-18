using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.Common.Application.Behaviours;
using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Common.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Assembly[] moduleAssemblies)
    {
        services.AddScoped<ISender, MessageDispatcher>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingPipelineBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestLoggingPipelineBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

        RegisterHandlers(services, moduleAssemblies);

        services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        Type[] handlerInterfaces =
        [
            typeof(IRequestHandler<,>)
        ];

        foreach (Assembly assembly in assemblies)
        {
            IEnumerable<Type> types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false });

            foreach (Type type in types)
            {
                foreach (Type implementedInterface in type.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType)
                    {
                        continue;
                    }

                    Type genericDef = implementedInterface.GetGenericTypeDefinition();

                    if (Array.Exists(handlerInterfaces, h => h == genericDef))
                    {
                        services.AddScoped(implementedInterface, type);
                    }
                }
            }
        }
    }
}
