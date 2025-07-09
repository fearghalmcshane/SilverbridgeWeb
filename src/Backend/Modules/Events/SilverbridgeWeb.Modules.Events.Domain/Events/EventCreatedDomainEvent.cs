using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Events.Domain.Events;

public sealed class EventCreatedDomainEvent(Guid eventId) : DomainEvent
{
    public Guid EventId { get; init; } = eventId;
}
