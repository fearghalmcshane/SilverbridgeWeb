using MediatR;
using Microsoft.Extensions.Logging;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Common.Application.Behaviours;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string moduleName = GetModuleName(typeof(TRequest).FullName!);
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("{ModuleName} Module: Processing request {RequestName}", moduleName, requestName);

        TResponse result = await next(cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("{ModuleName} Module: Completed request {RequestName}", moduleName, requestName);
        }
        else
        {
            logger.LogError("{ModuleName} Module: Completed request {RequestName} with error", moduleName, requestName);
        }

        return result;

    }

    private static string GetModuleName(string requestName) => requestName.Split('.')[2];
}
