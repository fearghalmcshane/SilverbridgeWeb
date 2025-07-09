using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Events.GetEvent;

public sealed record GetEventQuery(Guid EventId) : IQuery<EventResponse>;
