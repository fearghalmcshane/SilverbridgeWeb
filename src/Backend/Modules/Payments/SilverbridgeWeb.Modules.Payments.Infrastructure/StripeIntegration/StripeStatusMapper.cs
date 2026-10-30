using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;

internal static class StripeStatusMapper
{
    public static bool IsPaid(string? checkoutPaymentStatus) =>
        checkoutPaymentStatus is "paid" or "no_payment_required";

    /// <summary>
    /// Whether the payment behind a completed, unpaid checkout has failed rather than still being collected.
    /// A Direct Debit that is in flight is <c>processing</c>; one the bank refused goes back to needing a
    /// payment method. A first subscription invoice that is never paid leaves the subscription expired.
    /// </summary>
    public static bool IsFailedPayment(string? paymentIntentStatus, string? subscriptionStatus) =>
        paymentIntentStatus is "requires_payment_method" or "canceled" ||
        subscriptionStatus is "incomplete_expired";

    public static RefundStatus ToRefundStatus(string? status) =>
        status switch
        {
            "succeeded" => RefundStatus.Succeeded,
            "failed" or "canceled" => RefundStatus.Failed,
            _ => RefundStatus.Pending
        };

    /// <summary>Stripe keeps billing periods on the subscription's items; the latest one is when the next payment is due.</summary>
    public static DateTime? CurrentPeriodEnd(Stripe.Subscription subscription) =>
        subscription.Items?.Data?.Count > 0
            ? subscription.Items.Data.Max(item => item.CurrentPeriodEnd)
            : null;

    public static SubscriptionStatus ToSubscriptionStatus(string? status) =>
        status switch
        {
            "active" or "trialing" => SubscriptionStatus.Active,
            "past_due" or "unpaid" or "paused" => SubscriptionStatus.PastDue,
            "canceled" or "incomplete_expired" => SubscriptionStatus.Cancelled,
            _ => SubscriptionStatus.Incomplete
        };
}
