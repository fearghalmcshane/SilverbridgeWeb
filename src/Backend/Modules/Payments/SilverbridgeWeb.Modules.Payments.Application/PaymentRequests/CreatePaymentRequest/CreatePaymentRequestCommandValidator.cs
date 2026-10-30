using FluentValidation;
using SilverbridgeWeb.Modules.Payments.Domain;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CreatePaymentRequest;

internal sealed class CreatePaymentRequestCommandValidator : AbstractValidator<CreatePaymentRequestCommand>
{
    // £10,000 is far beyond anything the club charges and catches a unit mix-up (pounds sent as pence).
    internal const long MaxAmountMinor = 1_000_000;

    public CreatePaymentRequestCommandValidator()
    {
        RuleFor(c => c.PaymentRequestId).NotEmpty();
        RuleFor(c => c.PayerUserId).NotEmpty();
        RuleFor(c => c.Source).NotEmpty().MaximumLength(PaymentRequest.SourceMaxLength);
        RuleFor(c => c.SourceReference).NotEmpty().MaximumLength(PaymentRequest.SourceReferenceMaxLength);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(PaymentRequest.DescriptionMaxLength);
        RuleFor(c => c.Currency)
            .Must(Currencies.IsSupported)
            .WithMessage($"Only {Currencies.Gbp} payments are supported");
        RuleFor(c => c.Kind).IsInEnum();
        RuleFor(c => c.Lines).NotEmpty();
        RuleForEach(c => c.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Description).NotEmpty().MaximumLength(PaymentRequest.DescriptionMaxLength);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitAmountMinor).GreaterThan(0);
        });
        RuleFor(c => c.Lines)
            .Must(lines => lines.Sum(l => (decimal)l.Quantity * l.UnitAmountMinor) <= MaxAmountMinor)
            .When(c => c.Lines is { Count: > 0 })
            .WithMessage("The total amount is too large");
        RuleFor(c => c.Lines)
            .Must(lines => lines.Count == 1)
            .When(c => c.Kind == PaymentRequestKind.MonthlySubscription)
            .WithMessage("A monthly payment has exactly one line");
    }
}
