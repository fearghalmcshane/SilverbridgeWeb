using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Ticketing.Application.Tickets.GetTicket;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Tickets.GetTicketByCode;

public sealed record GetTicketByCodeQuery(string Code) : IQuery<TicketResponse>;
