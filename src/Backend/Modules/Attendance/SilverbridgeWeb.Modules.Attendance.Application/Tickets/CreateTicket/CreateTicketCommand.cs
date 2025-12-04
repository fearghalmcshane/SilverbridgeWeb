using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Attendance.Application.Tickets.CreateTicket;

public sealed record CreateTicketCommand(Guid TicketId, Guid AttendeeId, Guid EventId, string Code) : ICommand;
