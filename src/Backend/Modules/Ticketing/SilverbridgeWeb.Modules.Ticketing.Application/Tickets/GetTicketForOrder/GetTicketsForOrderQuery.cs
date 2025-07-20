using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Ticketing.Application.Tickets.GetTicket;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Tickets.GetTicketForOrder;

public sealed record GetTicketsForOrderQuery(Guid OrderId) : IQuery<IReadOnlyCollection<TicketResponse>>;
