using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Attendance.Application.Attendees.CreateAttendee;

public sealed record CreateAttendeeCommand(Guid AttendeeId, string Email, string FirstName, string LastName)
    : ICommand;
