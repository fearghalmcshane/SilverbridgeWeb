using MediatR;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Common.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
