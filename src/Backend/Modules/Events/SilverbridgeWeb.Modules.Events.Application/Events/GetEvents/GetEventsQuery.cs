using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Events.GetEvents;

public sealed record GetEventsQuery : IQuery<IReadOnlyCollection<EventResponse>>;
