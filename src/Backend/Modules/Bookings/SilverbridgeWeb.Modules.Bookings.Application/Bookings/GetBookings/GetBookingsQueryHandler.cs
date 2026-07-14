using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.GetBookings;

internal sealed class GetBookingsQueryHandler(IBookingRepository bookingRepository)
    : IQueryHandler<GetBookingsQuery, IReadOnlyCollection<BookingResponse>>
{
    public async Task<Result<IReadOnlyCollection<BookingResponse>>> Handle(
        GetBookingsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Booking> bookings = await bookingRepository.GetAllAsync(cancellationToken);

        IReadOnlyCollection<BookingResponse> response = bookings
            .Select(b => b.IsPublic
                ? new BookingResponse(b.Id, b.FacilityId, b.Title, b.BookerName, b.StartsAtUtc, b.EndsAtUtc, b.IsPublic, b.Status)
                : new BookingResponse(b.Id, b.FacilityId, "Private Booking", string.Empty, b.StartsAtUtc, b.EndsAtUtc, b.IsPublic, b.Status))
            .ToList();

        return Result.Success(response);
    }
}
