using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;

internal static class StripeStatusMapper
{
    public static bool IsPaid(string? checkoutPaymentStatus) =>
        checkoutPaymentStatus is "paid" or "no_payment_required";

    public static RefundStatus ToRefundStatus(string? status) =>
        status switch
        {
            "succeeded" => RefundStatus.Succeeded,
            "failed" or "canceled" => RefundStatus.Failed,
            _ => RefundStatus.Pending
        };

    public static SubscriptionStatus ToSubscriptionStatus(string? status) =>
        status switch
        {
            "active" or "trialing" => SubscriptionStatus.Active,
            "past_due" or "unpaid" or "paused" => SubscriptionStatus.PastDue,
            "canceled" or "incomplete_expired" => SubscriptionStatus.Cancelled,
            _ => SubscriptionStatus.Incomplete
        };
}
