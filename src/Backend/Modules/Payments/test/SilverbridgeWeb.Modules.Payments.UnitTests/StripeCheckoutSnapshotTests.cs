using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;
using Stripe;
using StripeCheckoutSession = Stripe.Checkout.Session;
using StripeSubscription = Stripe.Subscription;

namespace SilverbridgeWeb.Modules.Payments.UnitTests;

public sealed class StripeCheckoutSnapshotTests
{
    [Fact]
    public void CompletedDirectDebit_StillBeingCollected_IsNotFailed()
    {
        CheckoutSessionSnapshot snapshot = StripePaymentProvider.ToSnapshot(
            CompletedUnpaidSession(new PaymentIntent { Status = "processing" }));

        Assert.Equal(CheckoutSessionState.Complete, snapshot.State);
        Assert.False(snapshot.IsPaid);
        Assert.False(snapshot.PaymentFailed);
    }

    [Fact]
    public void CompletedDirectDebit_RefusedByTheBank_IsFailedWithItsReason()
    {
        CheckoutSessionSnapshot snapshot = StripePaymentProvider.ToSnapshot(
            CompletedUnpaidSession(new PaymentIntent
            {
                Status = "requires_payment_method",
                LastPaymentError = new StripeError { Message = "The account has insufficient funds." }
            }));

        Assert.True(snapshot.PaymentFailed);
        Assert.Equal("The account has insufficient funds.", snapshot.FailureReason);
    }

    [Fact]
    public void CompletedSubscription_WhoseFirstInvoiceWasNeverPaid_IsFailed()
    {
        StripeCheckoutSession session = CompletedUnpaidSession(paymentIntent: null);
        session.Subscription = new StripeSubscription { Id = "sub_1", Status = "incomplete_expired" };

        Assert.True(StripePaymentProvider.ToSnapshot(session).PaymentFailed);
    }

    private static StripeCheckoutSession CompletedUnpaidSession(PaymentIntent? paymentIntent) =>
        new()
        {
            Id = "cs_1",
            Status = "complete",
            PaymentStatus = "unpaid",
            PaymentIntent = paymentIntent
        };
}
