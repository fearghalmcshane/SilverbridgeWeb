using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

public static class BookingErrors
{
    public static readonly Error EndDatePrecedesStartDate = Error.Problem(
        "Bookings.EndDatePrecedesStartDate",
        "The booking end date must be after the start date.");

    public static readonly Error InvalidDateRange = Error.Problem(
        "Bookings.InvalidDateRange",
        "The date range must be valid and no longer than 366 days.");

    public static readonly Error InvalidRecurrence = Error.Problem(
        "Bookings.InvalidRecurrence",
        "Select valid recurrence days and an end date within 366 days of the start date.");

    public static readonly Error FacilityUnavailable = Error.Conflict(
        "Bookings.FacilityUnavailable",
        "The facility already has a booking during the selected time.");

    public static readonly Error NotPending = Error.Conflict(
        "Bookings.NotPending",
        "Only pending bookings can be approved.");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "Bookings.AlreadyCancelled",
        "The booking has already been deleted.");

    public static Error NotFound(Guid bookingId) =>
        Error.NotFound("Bookings.NotFound", $"Booking with id '{bookingId}' was not found.");
}
