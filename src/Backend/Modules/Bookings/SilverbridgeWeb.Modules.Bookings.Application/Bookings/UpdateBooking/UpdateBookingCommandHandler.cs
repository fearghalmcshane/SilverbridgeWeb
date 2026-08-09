using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.UpdateBooking;

internal sealed class UpdateBookingCommandHandler(
    IFacilityRepository facilityRepository,
    IBookingRepository bookingRepository)
    : ICommandHandler<UpdateBookingCommand>
{
    public async Task<Result> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        Booking? booking = await bookingRepository.GetAsync(request.BookingId, cancellationToken);
        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound(request.BookingId));
        }

        Facility? facility = await facilityRepository.GetAsync(request.FacilityId, cancellationToken);
        if (facility is null)
        {
            return Result.Failure(FacilityErrors.NotFound(request.FacilityId));
        }

        if (await bookingRepository.HasOverlapAsync(
                request.FacilityId,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.BookingId,
                cancellationToken))
        {
            return Result.Failure(BookingErrors.FacilityUnavailable);
        }

        Result result = booking.Update(
            facility,
            request.Title.Trim(),
            request.ContactName.Trim(),
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.IsPublic);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        if (!await bookingRepository.TryUpdateAsync(booking, cancellationToken))
        {
            return Result.Failure(BookingErrors.FacilityUnavailable);
        }

        return Result.Success();
    }
}
