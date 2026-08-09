using FluentValidation;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.UpdateBooking;

internal sealed class UpdateBookingCommandValidator : AbstractValidator<UpdateBookingCommand>
{
    public UpdateBookingCommandValidator()
    {
        RuleFor(c => c.BookingId).NotEmpty();
        RuleFor(c => c.FacilityId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(300);
        RuleFor(c => c.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.StartsAtUtc).NotEmpty();
        RuleFor(c => c.EndsAtUtc).NotEmpty().GreaterThan(c => c.StartsAtUtc)
            .WithMessage("End date must be after start date.");
    }
}
