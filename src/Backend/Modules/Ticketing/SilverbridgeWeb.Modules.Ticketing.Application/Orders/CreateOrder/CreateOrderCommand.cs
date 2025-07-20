using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(Guid CustomerId) : ICommand;
