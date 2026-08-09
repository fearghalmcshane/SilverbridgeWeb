using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.DeleteBooking;

internal sealed class DeleteBookingCommandHandler(
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteBookingCommand>
{
    public async Task<Result> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        Booking? booking = await bookingRepository.GetAsync(request.BookingId, cancellationToken);

        if (booking is null)
        {
            return Result.Failure(BookingErrors.NotFound(request.BookingId));
        }

        Result result = booking.Cancel();
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
