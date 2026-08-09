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

    public string ContactName { get; private set; }

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public bool IsPublic { get; private set; }

    public BookingStatus Status { get; private set; }

    public static Result<Booking> Create(
        Facility facility,
        string title,
        string bookerName,
        string contactName,
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
            ContactName = contactName,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            IsPublic = isPublic,
            Status = BookingStatus.Pending
        };
    }

    public Result Approve()
    {
        if (Status != BookingStatus.Pending)
        {
            return Result.Failure(BookingErrors.NotPending);
        }

        Status = BookingStatus.Confirmed;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            return Result.Failure(BookingErrors.AlreadyCancelled);
        }

        Status = BookingStatus.Cancelled;

        return Result.Success();
    }

    public Result Update(
        Facility facility,
        string title,
        string contactName,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isPublic)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            return Result.Failure(BookingErrors.EndDatePrecedesStartDate);
        }

        FacilityId = facility.Id;
        Title = title;
        ContactName = contactName;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        IsPublic = isPublic;

        return Result.Success();
    }
}
