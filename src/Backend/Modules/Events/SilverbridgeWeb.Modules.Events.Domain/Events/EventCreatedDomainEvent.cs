using SilverbridgeWeb.Modules.Events.Domain.Abstractions;

namespace SilverbridgeWeb.Modules.Events.Domain.Events;

public sealed class EventCreatedDomainEvent(Guid eventId) : DomainEvent
{
    public Guid EventId { get; init; } = eventId;
}
