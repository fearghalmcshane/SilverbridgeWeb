using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderResponse>;
