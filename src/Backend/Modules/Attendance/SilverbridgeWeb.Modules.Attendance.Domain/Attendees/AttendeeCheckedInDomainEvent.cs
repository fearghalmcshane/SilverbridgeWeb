using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Attendance.Domain.Attendees;

public sealed class AttendeeCheckedInDomainEvent(Guid attendeeId, Guid eventId) : DomainEvent
{
    public Guid AttendeeId { get; init; } = attendeeId;

    public Guid EventId { get; init; } = eventId;
}
