using SilverbridgeWeb.Modules.Events.Domain.Abstractions;

namespace SilverbridgeWeb.Modules.Events.Domain.TicketTypes;

public sealed class TicketTypeCreatedDomainEvent(Guid ticketTypeId) : DomainEvent
{
    public Guid TicketTypeId { get; init; } = ticketTypeId;
}
