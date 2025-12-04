using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Attendance.Application.Attendees.CheckInAttendee;

public sealed record CheckInAttendeeCommand(Guid AttendeeId, Guid TicketId) : ICommand;
