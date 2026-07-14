using FluentValidation;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

internal sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(c => c.FacilityId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(300);
        RuleFor(c => c.BookerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.StartsAtUtc).NotEmpty();
        RuleFor(c => c.EndsAtUtc).NotEmpty().GreaterThan(c => c.StartsAtUtc)
            .WithMessage("End date must be after start date.");
    }
}
