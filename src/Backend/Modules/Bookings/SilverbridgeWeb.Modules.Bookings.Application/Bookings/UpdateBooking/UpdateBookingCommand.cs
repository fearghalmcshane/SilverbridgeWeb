using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.UpdateBooking;

public sealed record UpdateBookingCommand(
    Guid BookingId,
    Guid FacilityId,
    string Title,
    string ContactName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsPublic) : ICommand;
