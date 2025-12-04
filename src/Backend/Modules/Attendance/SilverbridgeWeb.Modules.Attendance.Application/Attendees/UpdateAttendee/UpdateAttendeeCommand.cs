using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Attendance.Application.Attendees.UpdateAttendee;

public sealed record UpdateAttendeeCommand(Guid AttendeeId, string FirstName, string LastName) : ICommand;
