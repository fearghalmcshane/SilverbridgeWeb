using SilverbridgeWeb.Modules.Ticketing.Domain.Orders;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Orders.GetOrders;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    decimal TotalPrice,
    DateTime CreatedAtUtc);
