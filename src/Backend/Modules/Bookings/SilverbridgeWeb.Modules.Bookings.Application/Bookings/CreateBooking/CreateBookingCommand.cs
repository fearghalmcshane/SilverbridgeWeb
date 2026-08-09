using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

public sealed record CreateBookingCommand(
    Guid FacilityId,
    string Title,
    string BookerName,
    string ContactName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsPublic,
    bool IsRecurring,
    IReadOnlyCollection<DayOfWeek> RecurrenceDays,
    DateTime? RecurrenceEndDate) : ICommand<Guid>;
