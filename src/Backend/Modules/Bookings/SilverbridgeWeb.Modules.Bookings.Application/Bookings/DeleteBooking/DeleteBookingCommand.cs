using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.DeleteBooking;

public sealed record DeleteBookingCommand(Guid BookingId) : ICommand;
