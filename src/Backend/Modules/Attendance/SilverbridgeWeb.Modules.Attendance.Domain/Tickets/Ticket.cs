using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Attendance.Domain.Attendees;
using SilverbridgeWeb.Modules.Attendance.Domain.Events;

namespace SilverbridgeWeb.Modules.Attendance.Domain.Tickets;

public sealed class Ticket : Entity
{
    private Ticket()
    {
    }

    public Guid Id { get; private set; }

    public Guid AttendeeId { get; private set; }

    public Guid EventId { get; private set; }

    public string Code { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    public static Ticket Create(Guid ticketId, Attendee attendee, Event @event, string code)
    {
        var ticket = new Ticket
        {
            Id = ticketId,
            AttendeeId = attendee.Id,
            EventId = @event.Id,
            Code = code
        };

        ticket.Raise(new TicketCreatedDomainEvent(ticket.Id, ticket.EventId));

        return ticket;
    }

    internal void MarkAsUsed()
    {
        UsedAtUtc = DateTime.UtcNow;

        Raise(new TicketUsedDomainEvent(Id));
    }
}
