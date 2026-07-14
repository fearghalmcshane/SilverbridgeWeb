using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

internal sealed class CreateBookingCommandHandler(
    IFacilityRepository facilityRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBookingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        Facility? facility = await facilityRepository.GetAsync(request.FacilityId, cancellationToken);

        if (facility is null)
        {
            return Result.Failure<Guid>(FacilityErrors.NotFound(request.FacilityId));
        }

        Result<Booking> result = Booking.Create(
            facility,
            request.Title,
            request.BookerName,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.IsPublic);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        bookingRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
