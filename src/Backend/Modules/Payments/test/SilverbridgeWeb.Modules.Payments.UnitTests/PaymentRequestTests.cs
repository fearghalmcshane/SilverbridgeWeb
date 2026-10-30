using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Domain;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.UnitTests;

public sealed class PaymentRequestTests
{
    private static readonly DateTime Now = new(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_TotalsLinesInMinorUnits()
    {
        PaymentRequest request = CreateRequest(
            new PaymentRequestLineDetails("Adult ticket", 2, 1250),
            new PaymentRequestLineDetails("Child ticket", 1, 500));

        Assert.Equal(3000, request.AmountMinor);
        Assert.Equal(PaymentRequestStatus.Pending, request.Status);
        Assert.Empty(request.DomainEvents);
    }

    [Fact]
    public void Create_RejectsOtherCurrencies()
    {
        Result<PaymentRequest> result = PaymentRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), "test", "ref", "Test", "EUR", PaymentRequestKind.OneOff,
            [new PaymentRequestLineDetails("Line", 1, 100)], Now);

        Assert.Equal(PaymentRequestErrors.UnsupportedCurrency, result.Error);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(1, 0)]
    [InlineData(-1, 100)]
    public void Create_RejectsInvalidLines(int quantity, long unitAmountMinor)
    {
        Result<PaymentRequest> result = PaymentRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), "test", "ref", "Test", Currencies.Gbp, PaymentRequestKind.OneOff,
            [new PaymentRequestLineDetails("Line", quantity, unitAmountMinor)], Now);

        Assert.Equal(PaymentRequestErrors.InvalidLine, result.Error);
    }

    [Fact]
    public void CardCheckout_SucceedsOnceEvenWhenCompletedTwice()
    {
        PaymentRequest request = CreateAwaitingPayment();

        Assert.True(request.MarkCheckoutCompleted("cs_1", isPaid: true, "pi_1", "cus_1", null, Now).IsSuccess);
        Assert.True(request.MarkCheckoutCompleted("cs_1", isPaid: true, "pi_1", "cus_1", null, Now).IsSuccess);

        Assert.Equal(PaymentRequestStatus.Succeeded, request.Status);
        Assert.Equal("pi_1", request.ProviderPaymentIntentId);
        Assert.Single(request.DomainEvents.OfType<PaymentRequestSucceededDomainEvent>());
        Assert.Null(request.CheckoutUrl);
    }

    [Fact]
    public void DirectDebitCheckout_ProcessesThenSucceeds()
    {
        PaymentRequest request = CreateAwaitingPayment();

        request.MarkCheckoutCompleted("cs_1", isPaid: false, "pi_1", "cus_1", null, Now);
        Assert.Equal(PaymentRequestStatus.Processing, request.Status);
        Assert.Empty(request.DomainEvents);

        request.MarkAsyncPaymentSucceeded("pi_1", Now);
        Assert.Equal(PaymentRequestStatus.Succeeded, request.Status);
        Assert.Single(request.DomainEvents.OfType<PaymentRequestSucceededDomainEvent>());
    }

    [Fact]
    public void DirectDebitCheckout_CanFail()
    {
        PaymentRequest request = CreateAwaitingPayment();
        request.MarkCheckoutCompleted("cs_1", isPaid: false, "pi_1", null, null, Now);

        request.MarkAsyncPaymentFailed("Insufficient funds", Now);
        request.MarkAsyncPaymentFailed("Insufficient funds", Now);

        Assert.Equal(PaymentRequestStatus.Failed, request.Status);
        Assert.Equal("Insufficient funds", request.FailureReason);
        Assert.Single(request.DomainEvents.OfType<PaymentRequestFailedDomainEvent>());
    }

    [Fact]
    public void Expiry_OfTheCurrentSession_ClosesTheRequest()
    {
        PaymentRequest request = CreateAwaitingPayment();

        request.MarkCheckoutExpired("cs_1", Now);

        Assert.Equal(PaymentRequestStatus.Expired, request.Status);
        Assert.Single(request.DomainEvents.OfType<PaymentRequestExpiredDomainEvent>());
    }

    [Fact]
    public void Expiry_OfAReplacedSessionOrAfterPayment_IsIgnored()
    {
        PaymentRequest request = CreateAwaitingPayment();
        request.AttachCheckout("cs_2", new Uri("https://checkout.stripe.com/cs_2"), Now.AddMinutes(35), null, Now);

        request.MarkCheckoutExpired("cs_1", Now);
        Assert.Equal(PaymentRequestStatus.AwaitingPayment, request.Status);

        request.MarkCheckoutCompleted("cs_2", isPaid: true, "pi_1", null, null, Now);
        request.MarkCheckoutExpired("cs_2", Now);
        Assert.Equal(PaymentRequestStatus.Succeeded, request.Status);
        Assert.Empty(request.DomainEvents.OfType<PaymentRequestExpiredDomainEvent>());
    }

    [Fact]
    public void Completion_AfterCancellation_IsRejected()
    {
        PaymentRequest request = CreateAwaitingPayment();
        request.Cancel(Now);

        Result result = request.MarkCheckoutCompleted("cs_1", isPaid: true, "pi_1", null, null, Now);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void HasOpenCheckout_OnlyWhileTheSessionHasTimeLeft()
    {
        PaymentRequest request = CreateAwaitingPayment();

        Assert.True(request.HasOpenCheckout(Now));
        Assert.False(request.HasOpenCheckout(Now.AddMinutes(34)));
    }

    [Fact]
    public void Refund_CannotExceedWhatIsLeft()
    {
        PaymentRequest request = CreatePaid(amountMinor: 1000);

        Result<Refund> first = request.RequestRefund(Guid.NewGuid(), 600, "Partial", null, Now);
        Result<Refund> tooMuch = request.RequestRefund(Guid.NewGuid(), 500, "Too much", null, Now);

        Assert.True(first.IsSuccess);
        Assert.Equal(PaymentRequestErrors.InvalidRefundAmount, tooMuch.Error);
        Assert.Equal(400, request.RefundableAmountMinor);
    }

    [Fact]
    public void Refund_InFullMarksTheRequestRefunded()
    {
        PaymentRequest request = CreatePaid(amountMinor: 1000);
        Refund refund = request.RequestRefund(Guid.NewGuid(), null, "Event cancelled", null, Now).Value;

        request.ApplyRefundUpdate("re_1", refund.Id, refund.AmountMinor, RefundStatus.Succeeded, null, Now);
        request.ApplyRefundUpdate("re_1", refund.Id, refund.AmountMinor, RefundStatus.Succeeded, null, Now);

        Assert.Equal(1000, request.AmountRefundedMinor);
        Assert.Equal(PaymentRequestStatus.Refunded, request.Status);
        PaymentRefundedDomainEvent refunded = Assert.Single(request.DomainEvents.OfType<PaymentRefundedDomainEvent>());
        Assert.True(refunded.IsFullyRefunded);
    }

    [Fact]
    public void Refund_MadeInTheProviderDashboard_IsRecorded()
    {
        PaymentRequest request = CreatePaid(amountMinor: 1000);

        request.ApplyRefundUpdate("re_dashboard", null, 250, RefundStatus.Succeeded, null, Now);

        Refund refund = Assert.Single(request.Refunds);
        Assert.Equal("re_dashboard", refund.ProviderRefundId);
        Assert.Equal(250, request.AmountRefundedMinor);
        Assert.Equal(PaymentRequestStatus.Succeeded, request.Status);
    }

    [Fact]
    public void Refund_RequestedAgainWithTheSameId_ReturnsTheExistingRefund()
    {
        PaymentRequest request = CreatePaid(amountMinor: 1000);
        var refundId = Guid.NewGuid();

        Refund first = request.RequestRefund(refundId, 400, "Partial", null, Now).Value;
        Refund retried = request.RequestRefund(refundId, 400, "Partial", null, Now).Value;

        Assert.Same(first, retried);
        Assert.Single(request.Refunds);
        Assert.Equal(600, request.RefundableAmountMinor);
    }

    [Fact]
    public void Refund_SentButNeverSaved_IsRecordedUnderItsOwnIdByTheWebhook()
    {
        PaymentRequest request = CreatePaid(amountMinor: 1000);
        var refundId = Guid.NewGuid();

        request.ApplyRefundUpdate("re_1", refundId, 400, RefundStatus.Succeeded, null, Now);
        Refund retried = request.RequestRefund(refundId, 400, "Partial", null, Now).Value;

        Assert.Equal(refundId, Assert.Single(request.Refunds).Id);
        Assert.Equal("re_1", retried.ProviderRefundId);
        Assert.Equal(400, request.AmountRefundedMinor);
    }

    [Fact]
    public void Refund_ThatFails_ReleasesTheAmount()
    {
        PaymentRequest request = CreatePaid(amountMinor: 1000);
        Refund refund = request.RequestRefund(Guid.NewGuid(), 1000, "Duplicate", null, Now).Value;

        request.ApplyRefundUpdate("re_1", refund.Id, 1000, RefundStatus.Failed, "card_closed", Now);

        Assert.Equal(RefundStatus.Failed, refund.Status);
        Assert.Equal(0, request.AmountRefundedMinor);
        Assert.Equal(1000, request.RefundableAmountMinor);
        Assert.False(Assert.Single(request.DomainEvents.OfType<PaymentRefundFailedDomainEvent>()).WasReversed);
    }

    [Fact]
    public void Refund_ThatFailsAfterSucceeding_IsReversed()
    {
        PaymentRequest request = CreatePaid(amountMinor: 1000);
        Refund refund = request.RequestRefund(Guid.NewGuid(), null, "Event cancelled", null, Now).Value;
        request.ApplyRefundUpdate("re_1", refund.Id, 1000, RefundStatus.Succeeded, null, Now);

        // The payer's bank sent the money back.
        request.ApplyRefundUpdate("re_1", refund.Id, 1000, RefundStatus.Failed, "expired_or_canceled_card", Now);
        request.ApplyRefundUpdate("re_1", refund.Id, 1000, RefundStatus.Failed, "expired_or_canceled_card", Now);

        Assert.Equal(RefundStatus.Failed, refund.Status);
        Assert.Equal(0, request.AmountRefundedMinor);
        Assert.Equal(PaymentRequestStatus.Succeeded, request.Status);
        Assert.Equal(1000, request.RefundableAmountMinor);
        PaymentRefundFailedDomainEvent failed = Assert.Single(request.DomainEvents.OfType<PaymentRefundFailedDomainEvent>());
        Assert.True(failed.WasReversed);
        Assert.Equal(0, failed.TotalRefundedMinor);
        Assert.Equal("expired_or_canceled_card", failed.FailureReason);
    }

    [Fact]
    public void Refund_IsNotAvailableForMonthlyPayments()
    {
        PaymentRequest request = CreateRequest(PaymentRequestKind.MonthlySubscription, new PaymentRequestLineDetails("Lotto", 1, 1000));
        request.AttachCheckout("cs_1", new Uri("https://checkout.stripe.com/cs_1"), Now.AddMinutes(35), null, Now);
        request.MarkCheckoutCompleted("cs_1", isPaid: true, "pi_1", "cus_1", "sub_1", Now);

        Result<Refund> result = request.RequestRefund(Guid.NewGuid(), null, "No", null, Now);

        Assert.Equal(PaymentRequestErrors.NotRefundable, result.Error);
    }

    internal static PaymentRequest CreateRequest(params PaymentRequestLineDetails[] lines) =>
        CreateRequest(PaymentRequestKind.OneOff, lines);

    internal static PaymentRequest CreateRequest(PaymentRequestKind kind, params PaymentRequestLineDetails[] lines) =>
        PaymentRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), "test", "ref-1", "Club Keeley term", Currencies.Gbp, kind, lines, Now).Value;

    private static PaymentRequest CreateAwaitingPayment()
    {
        PaymentRequest request = CreateRequest(new PaymentRequestLineDetails("Term fee", 1, 4000));
        request.AttachCheckout("cs_1", new Uri("https://checkout.stripe.com/cs_1"), Now.AddMinutes(35), null, Now);
        return request;
    }

    private static PaymentRequest CreatePaid(long amountMinor)
    {
        PaymentRequest request = CreateRequest(new PaymentRequestLineDetails("Ticket", 1, amountMinor));
        request.AttachCheckout("cs_1", new Uri("https://checkout.stripe.com/cs_1"), Now.AddMinutes(35), null, Now);
        request.MarkCheckoutCompleted("cs_1", isPaid: true, "pi_1", "cus_1", null, Now);
        request.ClearDomainEvents();
        return request;
    }
}
