using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Ticketing.Domain.Events;

public sealed class EventTicketsArchivedDomainEvent(Guid eventId) : DomainEvent
{
    public Guid EventId { get; init; } = eventId;
}
