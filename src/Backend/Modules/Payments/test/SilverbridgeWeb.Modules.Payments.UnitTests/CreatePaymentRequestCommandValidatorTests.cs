using FluentValidation.Results;
using SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CreatePaymentRequest;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.UnitTests;

public sealed class CreatePaymentRequestCommandValidatorTests
{
    private readonly CreatePaymentRequestCommandValidator _validator = new();

    [Fact]
    public void AcceptsAWellFormedRequest()
    {
        ValidationResult result = _validator.Validate(Command());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RejectsNonSterlingCurrency()
    {
        ValidationResult result = _validator.Validate(Command() with { Currency = "EUR" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreatePaymentRequestCommand.Currency));
    }

    [Fact]
    public void RejectsTotalsThatLookLikePoundsSentAsPence()
    {
        ValidationResult result = _validator.Validate(Command() with
        {
            Lines = [new PaymentRequestLineDetails("Ticket", 2, CreatePaymentRequestCommandValidator.MaxAmountMinor)]
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void RejectsMonthlyPaymentsWithSeveralLines()
    {
        ValidationResult result = _validator.Validate(Command() with
        {
            Kind = PaymentRequestKind.MonthlySubscription,
            Lines = [new PaymentRequestLineDetails("A", 1, 100), new PaymentRequestLineDetails("B", 1, 100)]
        });

        Assert.False(result.IsValid);
    }

    private static CreatePaymentRequestCommand Command() =>
        new(
            Guid.NewGuid(),
            "ticketing",
            "order-1",
            Guid.NewGuid(),
            "Senior final tickets",
            "GBP",
            PaymentRequestKind.OneOff,
            [new PaymentRequestLineDetails("Adult", 2, 1000)]);
}
