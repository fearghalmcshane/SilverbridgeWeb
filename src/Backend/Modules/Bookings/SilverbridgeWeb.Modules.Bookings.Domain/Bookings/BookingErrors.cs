using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

public static class BookingErrors
{
    public static readonly Error EndDatePrecedesStartDate = Error.Problem(
        "Bookings.EndDatePrecedesStartDate",
        "The booking end date must be after the start date.");

    public static Error NotFound(Guid bookingId) =>
        Error.NotFound("Bookings.NotFound", $"Booking with id '{bookingId}' was not found.");
}
