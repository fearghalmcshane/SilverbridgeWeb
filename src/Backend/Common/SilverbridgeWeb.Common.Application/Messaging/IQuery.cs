using MediatR;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
