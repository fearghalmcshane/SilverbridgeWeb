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
        if (request.ToUtc <= request.FromUtc || request.ToUtc - request.FromUtc > TimeSpan.FromDays(366))
        {
            return Result.Failure<IReadOnlyCollection<BookingResponse>>(BookingErrors.InvalidDateRange);
        }

        IReadOnlyCollection<Booking> bookings = await bookingRepository.GetForRangeAsync(
            request.FromUtc,
            request.ToUtc,
            cancellationToken);

        IReadOnlyCollection<BookingResponse> response = bookings
            .Select(b => b.IsPublic || request.IncludePrivateDetails
                ? new BookingResponse(
                    b.Id,
                    b.FacilityId,
                    b.Title,
                    b.BookerName,
                    b.ContactName,
                    b.StartsAtUtc,
                    b.EndsAtUtc,
                    b.IsPublic,
                    b.Status)
                : new BookingResponse(
                    b.Id,
                    b.FacilityId,
                    "Private Booking",
                    string.Empty,
                    string.Empty,
                    b.StartsAtUtc,
                    b.EndsAtUtc,
                    b.IsPublic,
                    b.Status))
            .ToList();

        return Result.Success(response);
    }
}
