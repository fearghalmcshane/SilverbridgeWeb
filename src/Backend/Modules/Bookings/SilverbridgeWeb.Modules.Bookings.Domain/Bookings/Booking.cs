using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

public sealed class Booking : Entity
{
    private Booking()
    {
    }

    public Guid Id { get; private set; }

    public Guid FacilityId { get; private set; }

    public string Title { get; private set; }

    public string BookerName { get; private set; }

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public bool IsPublic { get; private set; }

    public BookingStatus Status { get; private set; }

    public static Result<Booking> Create(
        Facility facility,
        string title,
        string bookerName,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isPublic)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            return Result.Failure<Booking>(BookingErrors.EndDatePrecedesStartDate);
        }

        return new Booking
        {
            Id = Ulid.NewUlid().ToGuid(),
            FacilityId = facility.Id,
            Title = title,
            BookerName = bookerName,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            IsPublic = isPublic,
            Status = BookingStatus.Pending
        };
    }
}
