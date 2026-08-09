using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.ApproveBooking;

public sealed record ApproveBookingCommand(Guid BookingId) : ICommand;
