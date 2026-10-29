using FluentValidation;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;

internal sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(c => c.PaymentRequestId).NotEmpty();
        RuleFor(c => c.AmountMinor).GreaterThan(0).When(c => c.AmountMinor is not null);
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(PaymentRequest.ReasonMaxLength);
    }
}
