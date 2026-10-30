using System.Globalization;

namespace SilverbridgeWeb.Modules.Payments.UnitTests;

/// <summary>Minimal Stripe webhook payloads in the shape Stripe sends them.</summary>
internal static class StripeEvents
{
    private const string ApiVersion = "2026-09-30.endive";

    public static string CheckoutSession(
        string eventId,
        string type,
        Guid paymentRequestId,
        string sessionId = "cs_test_1",
        string paymentStatus = "paid",
        string status = "complete",
        string? subscriptionId = null) =>
        Envelope(
            eventId,
            type,
            $$"""
              {
                "id": "{{sessionId}}",
                "object": "checkout.session",
                "client_reference_id": "{{paymentRequestId}}",
                "customer": "cus_test_1",
                "metadata": { "payment_request_id": "{{paymentRequestId}}" },
                "mode": "{{(subscriptionId is null ? "payment" : "subscription")}}",
                "payment_intent": {{(subscriptionId is null ? "\"pi_test_1\"" : "null")}},
                "payment_status": "{{paymentStatus}}",
                "status": "{{status}}",
                "subscription": {{(subscriptionId is null ? "null" : $"\"{subscriptionId}\"")}},
                "expires_at": 1793300000,
                "url": null
              }
              """);

    public static string Refund(
        string eventId,
        string type,
        string refundId,
        long amount,
        string status,
        Guid? paymentRequestId = null,
        Guid? ourRefundId = null)
    {
        string metadata = paymentRequestId is null
            ? "{}"
            : $$"""{ "payment_request_id": "{{paymentRequestId}}", "refund_id": "{{ourRefundId}}" }""";

        return Envelope(
            eventId,
            type,
            $$"""
              {
                "id": "{{refundId}}",
                "object": "refund",
                "amount": {{amount.ToString(CultureInfo.InvariantCulture)}},
                "currency": "gbp",
                "metadata": {{metadata}},
                "payment_intent": "pi_test_1",
                "status": "{{status}}"
              }
              """);
    }

    public static string Subscription(string eventId, string type, string subscriptionId, string status, Guid paymentRequestId) =>
        Envelope(
            eventId,
            type,
            $$"""
              {
                "id": "{{subscriptionId}}",
                "object": "subscription",
                "customer": "cus_test_1",
                "metadata": { "payment_request_id": "{{paymentRequestId}}" },
                "status": "{{status}}",
                "items": {
                  "object": "list",
                  "data": [
                    { "id": "si_1", "object": "subscription_item", "current_period_start": 1793300000, "current_period_end": 1795978400 }
                  ],
                  "has_more": false,
                  "url": "/v1/subscription_items"
                }
              }
              """);

    public static string Invoice(string eventId, string type, string invoiceId, string? subscriptionId, long amount) =>
        Envelope(
            eventId,
            type,
            $$"""
              {
                "id": "{{invoiceId}}",
                "object": "invoice",
                "amount_due": {{amount.ToString(CultureInfo.InvariantCulture)}},
                "amount_paid": {{(type == "invoice.paid" ? amount : 0).ToString(CultureInfo.InvariantCulture)}},
                "currency": "gbp",
                "parent": {{(subscriptionId is null
                    ? "null"
                    : $$"""{ "type": "subscription_details", "subscription_details": { "subscription": "{{subscriptionId}}", "metadata": {} } }""")}},
                "lines": {
                  "object": "list",
                  "data": [
                    { "id": "il_1", "object": "line_item", "period": { "start": 1793300000, "end": 1795978400 } }
                  ],
                  "has_more": false,
                  "url": "/v1/invoices/{{invoiceId}}/lines"
                }
              }
              """);

    private static string Envelope(string eventId, string type, string dataObject) =>
        $$"""
          {
            "id": "{{eventId}}",
            "object": "event",
            "api_version": "{{ApiVersion}}",
            "created": 1793300000,
            "livemode": false,
            "pending_webhooks": 1,
            "request": { "id": null, "idempotency_key": null },
            "type": "{{type}}",
            "data": { "object": {{dataObject}} }
          }
          """;
}
