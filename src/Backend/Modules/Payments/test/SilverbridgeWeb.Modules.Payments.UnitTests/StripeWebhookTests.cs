using Microsoft.Extensions.Options;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure;
using SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;
using Stripe;

namespace SilverbridgeWeb.Modules.Payments.UnitTests;

public sealed class StripeWebhookTests
{
    private const string Secret = "whsec_unit_test";
    private static readonly DateTime Now = new(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Verifier_AcceptsCorrectlySignedEvents()
    {
        string payload = StripeEvents.CheckoutSession("evt_1", "checkout.session.completed", Guid.NewGuid());

        Result<VerifiedProviderWebhook> result = CreateVerifier().Verify(payload, Sign(payload, Now));

        Assert.True(result.IsSuccess);
        Assert.Equal(new VerifiedProviderWebhook("evt_1", "checkout.session.completed"), result.Value);
    }

    [Fact]
    public void Verifier_RejectsTamperedStaleOrUnsignedEvents()
    {
        string payload = StripeEvents.CheckoutSession("evt_1", "checkout.session.completed", Guid.NewGuid());
        StripeWebhookVerifier verifier = CreateVerifier();

        Assert.True(verifier.Verify(payload.Replace("evt_1", "evt_2", StringComparison.Ordinal), Sign(payload, Now)).IsFailure);
        Assert.True(verifier.Verify(payload, Sign(payload, Now.AddMinutes(-10))).IsFailure);
        Assert.True(verifier.Verify(payload, null).IsFailure);
        Assert.True(verifier.Verify(payload, "t=1,v1=not-a-signature").IsFailure);
    }

    [Fact]
    public void Parser_ReadsCompletedCardCheckout()
    {
        var paymentRequestId = Guid.NewGuid();

        ProviderEvent? parsed = StripeWebhookEventParser.Parse(
            StripeEvents.CheckoutSession("evt_1", "checkout.session.completed", paymentRequestId));

        CheckoutCompletedProviderEvent completed = Assert.IsType<CheckoutCompletedProviderEvent>(parsed);
        Assert.Equal(paymentRequestId, completed.PaymentRequestId);
        Assert.Equal("cs_test_1", completed.SessionId);
        Assert.True(completed.IsPaid);
        Assert.Equal("pi_test_1", completed.PaymentIntentId);
        Assert.Equal("cus_test_1", completed.CustomerId);
        Assert.Null(completed.SubscriptionId);
    }

    [Fact]
    public void Parser_ReadsDirectDebitCheckoutAsNotYetPaid()
    {
        ProviderEvent? parsed = StripeWebhookEventParser.Parse(StripeEvents.CheckoutSession(
            "evt_1", "checkout.session.completed", Guid.NewGuid(), paymentStatus: "unpaid", subscriptionId: "sub_1"));

        CheckoutCompletedProviderEvent completed = Assert.IsType<CheckoutCompletedProviderEvent>(parsed);
        Assert.False(completed.IsPaid);
        Assert.Equal("sub_1", completed.SubscriptionId);
    }

    [Theory]
    [InlineData("checkout.session.async_payment_succeeded", typeof(AsyncPaymentSucceededProviderEvent))]
    [InlineData("checkout.session.async_payment_failed", typeof(AsyncPaymentFailedProviderEvent))]
    [InlineData("checkout.session.expired", typeof(CheckoutExpiredProviderEvent))]
    public void Parser_ReadsOtherCheckoutEvents(string type, Type expected)
    {
        ProviderEvent? parsed = StripeWebhookEventParser.Parse(StripeEvents.CheckoutSession("evt_1", type, Guid.NewGuid()));

        Assert.IsType(expected, parsed);
    }

    [Fact]
    public void Parser_ReadsRefunds()
    {
        var paymentRequestId = Guid.NewGuid();
        var refundId = Guid.NewGuid();

        ProviderEvent? parsed = StripeWebhookEventParser.Parse(StripeEvents.Refund(
            "evt_1", "refund.updated", "re_1", 500, "succeeded", paymentRequestId, refundId));

        RefundUpdatedProviderEvent refund = Assert.IsType<RefundUpdatedProviderEvent>(parsed);
        Assert.Equal("re_1", refund.ProviderRefundId);
        Assert.Equal(paymentRequestId, refund.PaymentRequestId);
        Assert.Equal(refundId, refund.RefundId);
        Assert.Equal(500, refund.AmountMinor);
        Assert.Equal(RefundStatus.Succeeded, refund.Status);
        Assert.Equal("pi_test_1", refund.PaymentIntentId);
    }

    [Fact]
    public void Parser_ReadsSubscriptionStatusAndPeriod()
    {
        var paymentRequestId = Guid.NewGuid();

        ProviderEvent? parsed = StripeWebhookEventParser.Parse(StripeEvents.Subscription(
            "evt_1", "customer.subscription.updated", "sub_1", "past_due", paymentRequestId));

        SubscriptionUpdatedProviderEvent subscription = Assert.IsType<SubscriptionUpdatedProviderEvent>(parsed);
        Assert.Equal(SubscriptionStatus.PastDue, subscription.Status);
        Assert.Equal(paymentRequestId, subscription.PaymentRequestId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1795978400).UtcDateTime, subscription.CurrentPeriodEndUtc);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1793300000).UtcDateTime, subscription.OccurredAtUtc);
        Assert.Equal(DateTimeKind.Utc, subscription.OccurredAtUtc.Kind);
    }

    [Fact]
    public void Parser_TreatsDeletedSubscriptionsAsCancelled()
    {
        ProviderEvent? parsed = StripeWebhookEventParser.Parse(StripeEvents.Subscription(
            "evt_1", "customer.subscription.deleted", "sub_1", "active", Guid.NewGuid()));

        Assert.Equal(SubscriptionStatus.Cancelled, Assert.IsType<SubscriptionUpdatedProviderEvent>(parsed).Status);
    }

    [Fact]
    public void Parser_ReadsSubscriptionInvoices()
    {
        ProviderEvent? paid = StripeWebhookEventParser.Parse(StripeEvents.Invoice("evt_1", "invoice.paid", "in_1", "sub_1", 1500));
        ProviderEvent? failed = StripeWebhookEventParser.Parse(StripeEvents.Invoice("evt_2", "invoice.payment_failed", "in_2", "sub_1", 1500));

        InvoicePaymentProviderEvent paidInvoice = Assert.IsType<InvoicePaymentProviderEvent>(paid);
        Assert.True(paidInvoice.Succeeded);
        Assert.Equal(1500, paidInvoice.AmountMinor);
        Assert.Equal("sub_1", paidInvoice.ProviderSubscriptionId);
        Assert.NotNull(paidInvoice.PeriodEndUtc);
        Assert.False(Assert.IsType<InvoicePaymentProviderEvent>(failed).Succeeded);
    }

    [Fact]
    public void Parser_IgnoresInvoicesWithoutASubscriptionAndUnknownEvents()
    {
        Assert.Null(StripeWebhookEventParser.Parse(StripeEvents.Invoice("evt_1", "invoice.paid", "in_1", null, 1500)));
        Assert.Null(StripeWebhookEventParser.Parse(
            StripeEvents.CheckoutSession("evt_1", "checkout.session.async_payment_succeeded", Guid.NewGuid())
                .Replace("checkout.session.async_payment_succeeded", "customer.created", StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(1, 15)]
    [InlineData(2, 30)]
    [InlineData(5, 240)]
    [InlineData(20, 3600)]
    public void RetryDelay_GrowsAndIsCapped(int attempt, int expectedSeconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), ProcessProviderWebhooksJob.RetryDelay(attempt));
    }

    private static StripeWebhookVerifier CreateVerifier() =>
        new(
            Options.Create(new PaymentsOptions { Stripe = new StripeOptions { WebhookSigningSecret = Secret } }),
            new FixedClock());

    private static string Sign(string payload, DateTime signedAtUtc) =>
        EventUtility.GenerateSignatureHeader(payload, Secret, new DateTimeOffset(signedAtUtc).ToUnixTimeSeconds());

    private sealed class FixedClock : IDateTimeProvider
    {
        public DateTime UtcNow => Now;
    }
}
