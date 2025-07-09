using MediatR;
using Microsoft.Extensions.Logging;
using SilverbridgeWeb.Common.Application.Exceptions;

namespace SilverbridgeWeb.Common.Application.Behaviours;

internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TRequest).Name);

            throw new SilverbridgeWebException(typeof(TRequest).Name, innerException: exception);
        }
    }
}
