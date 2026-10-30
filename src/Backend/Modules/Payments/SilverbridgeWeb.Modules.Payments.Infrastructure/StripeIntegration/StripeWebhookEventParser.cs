using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using Stripe;
using StripeCheckoutSession = Stripe.Checkout.Session;
using StripeRefund = Stripe.Refund;
using StripeSubscription = Stripe.Subscription;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;

/// <summary>Translates the Stripe events we subscribe to into provider-neutral <see cref="ProviderEvent"/>s.</summary>
internal static class StripeWebhookEventParser
{
    /// <summary>The event types to enable on the Stripe webhook endpoint.</summary>
    public static readonly IReadOnlyList<string> HandledEventTypes =
    [
        "checkout.session.completed",
        "checkout.session.async_payment_succeeded",
        "checkout.session.async_payment_failed",
        "checkout.session.expired",
        "refund.created",
        "refund.updated",
        "refund.failed",
        "customer.subscription.created",
        "customer.subscription.updated",
        "customer.subscription.deleted",
        "invoice.paid",
        "invoice.payment_failed"
    ];

    /// <returns>
    /// The translated event, or <see langword="null"/> when the event is of no interest, such as an event type we
    /// do not handle or a checkout that was not started by this site.
    /// </returns>
    public static ProviderEvent? Parse(string payload)
    {
        Event stripeEvent = EventUtility.ParseEvent(payload, throwOnApiVersionMismatch: false);

        return stripeEvent.Type switch
        {
            "checkout.session.completed" or
            "checkout.session.async_payment_succeeded" or
            "checkout.session.async_payment_failed" or
            "checkout.session.expired" => ParseCheckoutSession(stripeEvent),
            "refund.created" or "refund.updated" or "refund.failed" => ParseRefund(stripeEvent),
            "customer.subscription.created" or
            "customer.subscription.updated" or
            "customer.subscription.deleted" => ParseSubscription(stripeEvent),
            "invoice.paid" or "invoice.payment_failed" => ParseInvoice(stripeEvent),
            _ => null
        };
    }

    private static ProviderEvent? ParseCheckoutSession(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not StripeCheckoutSession session)
        {
            return null;
        }

        Guid? paymentRequestId =
            StripeMetadata.ReadGuid(session.Metadata, StripeMetadata.PaymentRequestId) ??
            (Guid.TryParse(session.ClientReferenceId, out Guid clientReference) ? clientReference : null);

        if (paymentRequestId is null)
        {
            return null;
        }

        return stripeEvent.Type switch
        {
            "checkout.session.completed" => new CheckoutCompletedProviderEvent(
                stripeEvent.Id,
                paymentRequestId.Value,
                session.Id,
                StripeStatusMapper.IsPaid(session.PaymentStatus),
                session.PaymentIntentId,
                session.CustomerId,
                session.SubscriptionId),
            "checkout.session.async_payment_succeeded" => new AsyncPaymentSucceededProviderEvent(
                stripeEvent.Id,
                paymentRequestId.Value,
                session.Id,
                session.PaymentIntentId),
            "checkout.session.async_payment_failed" => new AsyncPaymentFailedProviderEvent(
                stripeEvent.Id,
                paymentRequestId.Value,
                session.Id),
            _ => new CheckoutExpiredProviderEvent(stripeEvent.Id, paymentRequestId.Value, session.Id)
        };
    }

    private static RefundUpdatedProviderEvent? ParseRefund(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not StripeRefund refund)
        {
            return null;
        }

        RefundStatus status = stripeEvent.Type == "refund.failed"
            ? RefundStatus.Failed
            : StripeStatusMapper.ToRefundStatus(refund.Status);

        return new RefundUpdatedProviderEvent(
            stripeEvent.Id,
            refund.Id,
            StripeMetadata.ReadGuid(refund.Metadata, StripeMetadata.PaymentRequestId),
            StripeMetadata.ReadGuid(refund.Metadata, StripeMetadata.RefundId),
            refund.PaymentIntentId,
            refund.Amount,
            status,
            refund.FailureReason);
    }

    private static SubscriptionUpdatedProviderEvent? ParseSubscription(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not StripeSubscription subscription)
        {
            return null;
        }

        SubscriptionStatus status = stripeEvent.Type == "customer.subscription.deleted"
            ? SubscriptionStatus.Cancelled
            : StripeStatusMapper.ToSubscriptionStatus(subscription.Status);

        return new SubscriptionUpdatedProviderEvent(
            stripeEvent.Id,
            subscription.Id,
            StripeMetadata.ReadGuid(subscription.Metadata, StripeMetadata.PaymentRequestId),
            subscription.CustomerId,
            status,
            StripeStatusMapper.CurrentPeriodEnd(subscription),
            DateTime.SpecifyKind(stripeEvent.Created, DateTimeKind.Utc));
    }

    private static InvoicePaymentProviderEvent? ParseInvoice(Event stripeEvent)
    {
        // Only invoices raised by a subscription concern us; one-off payments are tracked through checkout.
        if (stripeEvent.Data.Object is not Invoice invoice ||
            invoice.Parent?.SubscriptionDetails?.SubscriptionId is not { } subscriptionId)
        {
            return null;
        }

        bool succeeded = stripeEvent.Type == "invoice.paid";
        InvoiceLineItemPeriod? period = invoice.Lines?.Data?.FirstOrDefault()?.Period;

        return new InvoicePaymentProviderEvent(
            stripeEvent.Id,
            invoice.Id,
            subscriptionId,
            succeeded ? invoice.AmountPaid : invoice.AmountDue,
            succeeded,
            period?.Start,
            period?.End,
            succeeded ? null : invoice.LastFinalizationError?.Message ?? "The payment could not be collected");
    }
}
