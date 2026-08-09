using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

internal sealed class CreateBookingCommandHandler(
    IFacilityRepository facilityRepository,
    IBookingRepository bookingRepository)
    : ICommandHandler<CreateBookingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        Facility? facility = await facilityRepository.GetAsync(request.FacilityId, cancellationToken);

        if (facility is null)
        {
            return Result.Failure<Guid>(FacilityErrors.NotFound(request.FacilityId));
        }

        Result<IReadOnlyCollection<BookingOccurrence>> scheduleResult = BookingRecurrence.CreateSchedule(
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.IsRecurring,
            request.RecurrenceDays,
            request.RecurrenceEndDate);

        if (scheduleResult.IsFailure)
        {
            return Result.Failure<Guid>(scheduleResult.Error);
        }

        IReadOnlyCollection<Booking> existingBookings =
            await bookingRepository.GetForFacilityRangeAsync(
                request.FacilityId,
                scheduleResult.Value.Min(occurrence => occurrence.StartsAtUtc),
                scheduleResult.Value.Max(occurrence => occurrence.EndsAtUtc),
                cancellationToken);

        if (scheduleResult.Value.Any(occurrence =>
                existingBookings.Any(booking =>
                    booking.StartsAtUtc < occurrence.EndsAtUtc &&
                    booking.EndsAtUtc > occurrence.StartsAtUtc)))
        {
            return Result.Failure<Guid>(BookingErrors.FacilityUnavailable);
        }

        var bookings = new List<Booking>();

        foreach (BookingOccurrence occurrence in scheduleResult.Value)
        {
            Result<Booking> bookingResult = Booking.Create(
                facility,
                request.Title.Trim(),
                request.BookerName.Trim(),
                request.ContactName.Trim(),
                occurrence.StartsAtUtc,
                occurrence.EndsAtUtc,
                request.IsPublic);

            if (bookingResult.IsFailure)
            {
                return Result.Failure<Guid>(bookingResult.Error);
            }

            bookings.Add(bookingResult.Value);
        }

        if (!await bookingRepository.TryInsertRangeAsync(bookings, cancellationToken))
        {
            return Result.Failure<Guid>(BookingErrors.FacilityUnavailable);
        }

        return bookings[0].Id;
    }
}
