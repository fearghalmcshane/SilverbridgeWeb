using FluentValidation;

namespace SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;

internal sealed class StartCheckoutCommandValidator : AbstractValidator<StartCheckoutCommand>
{
    public StartCheckoutCommandValidator()
    {
        RuleFor(c => c.PaymentRequestId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
    }
}
