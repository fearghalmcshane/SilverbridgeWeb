namespace SilverbridgeWeb.Common.Application.Messaging;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken = default);
