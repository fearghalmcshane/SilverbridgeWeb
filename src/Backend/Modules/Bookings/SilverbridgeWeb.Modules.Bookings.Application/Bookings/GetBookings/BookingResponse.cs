using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.GetBookings;

public sealed record BookingResponse(
    Guid Id,
    Guid FacilityId,
    string Title,
    string BookerName,
    string ContactName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsPublic,
    BookingStatus Status);
