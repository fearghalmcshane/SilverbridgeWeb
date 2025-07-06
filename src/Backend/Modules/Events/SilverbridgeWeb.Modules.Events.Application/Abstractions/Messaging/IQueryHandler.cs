using MediatR;
using SilverbridgeWeb.Modules.Events.Domain.Abstractions;

namespace SilverbridgeWeb.Modules.Events.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
