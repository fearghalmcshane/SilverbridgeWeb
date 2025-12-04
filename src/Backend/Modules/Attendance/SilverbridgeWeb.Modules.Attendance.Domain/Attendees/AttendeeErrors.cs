using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Attendance.Domain.Attendees;

public static class AttendeeErrors
{
    public static Error NotFound(Guid attendeeId) =>
        Error.NotFound("Attendees.NotFound", $"The attendee with the identifier {attendeeId} was not found");
}
