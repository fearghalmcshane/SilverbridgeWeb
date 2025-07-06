using SilverbridgeWeb.Modules.Events.Application.Abstractions.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Events.GetEvents;

public sealed record GetEventsQuery : IQuery<IReadOnlyCollection<EventResponse>>;
