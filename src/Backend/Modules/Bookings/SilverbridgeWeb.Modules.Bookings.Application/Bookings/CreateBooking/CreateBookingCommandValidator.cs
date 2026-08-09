using FluentValidation;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

internal sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(c => c.FacilityId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(300);
        RuleFor(c => c.BookerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.StartsAtUtc).NotEmpty();
        RuleFor(c => c.EndsAtUtc).NotEmpty().GreaterThan(c => c.StartsAtUtc)
            .WithMessage("End date must be after start date.");
        RuleFor(c => c.RecurrenceDays)
            .NotEmpty()
            .When(c => c.IsRecurring)
            .WithMessage("Select at least one recurrence day.");
        RuleFor(c => c.RecurrenceEndDate)
            .NotNull()
            .When(c => c.IsRecurring)
            .WithMessage("A recurrence end date is required.");
    }
}
