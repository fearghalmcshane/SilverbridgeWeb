using MediatR;

namespace SilverbridgeWeb.Modules.Events.Application.Events.GetEvent;

public sealed record GetEventQuery(Guid EventId) : IRequest<EventResponse?>;
