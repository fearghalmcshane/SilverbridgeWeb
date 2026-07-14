using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

public sealed record CreateBookingCommand(
    Guid FacilityId,
    string Title,
    string BookerName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsPublic) : ICommand<Guid>;
