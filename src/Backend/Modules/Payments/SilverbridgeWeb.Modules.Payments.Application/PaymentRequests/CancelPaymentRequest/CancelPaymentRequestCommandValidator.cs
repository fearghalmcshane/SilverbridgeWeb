using FluentValidation;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CancelPaymentRequest;

internal sealed class CancelPaymentRequestCommandValidator : AbstractValidator<CancelPaymentRequestCommand>
{
    public CancelPaymentRequestCommandValidator()
    {
        RuleFor(c => c.PaymentRequestId).NotEmpty();
    }
}
