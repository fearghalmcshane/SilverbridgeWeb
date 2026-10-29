using FluentValidation;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CancelSubscription;

internal sealed class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(c => c.SubscriptionId).NotEmpty();
    }
}
