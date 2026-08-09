using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.GetBookings;

public sealed record GetBookingsQuery(
    DateTime FromUtc,
    DateTime ToUtc,
    bool IncludePrivateDetails) : IQuery<IReadOnlyCollection<BookingResponse>>;
