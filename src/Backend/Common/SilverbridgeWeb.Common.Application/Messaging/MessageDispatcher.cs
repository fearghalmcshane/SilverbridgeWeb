using Microsoft.Extensions.DependencyInjection;

namespace SilverbridgeWeb.Common.Application.Messaging;

internal sealed class MessageDispatcher(IServiceProvider serviceProvider) : ISender
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Type requestType = request.GetType();
        Type responseType = typeof(TResponse);

        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        object handler = serviceProvider.GetRequiredService(handlerType);

        IEnumerable<object> behaviors = serviceProvider
            .GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType))
            .OfType<object>()
            .Reverse();

        RequestHandlerDelegate<TResponse> pipeline = (ct) =>
        {
            System.Reflection.MethodInfo method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;
            return (Task<TResponse>)method.Invoke(handler, [request, ct])!;
        };

        foreach (object behavior in behaviors)
        {
            RequestHandlerDelegate<TResponse> next = pipeline;
            Type behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            System.Reflection.MethodInfo handleMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!;

            pipeline = ct =>
                (Task<TResponse>)handleMethod.Invoke(behavior, [request, next, ct])!;
        }

        return pipeline(cancellationToken);
    }
}
