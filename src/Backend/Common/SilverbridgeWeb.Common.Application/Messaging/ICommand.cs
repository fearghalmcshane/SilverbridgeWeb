using MediatR;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Common.Application.Messaging;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand;
