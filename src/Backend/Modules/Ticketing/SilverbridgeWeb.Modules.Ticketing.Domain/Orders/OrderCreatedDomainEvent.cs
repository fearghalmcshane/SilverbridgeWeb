using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Ticketing.Domain.Orders;

public sealed class OrderCreatedDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
