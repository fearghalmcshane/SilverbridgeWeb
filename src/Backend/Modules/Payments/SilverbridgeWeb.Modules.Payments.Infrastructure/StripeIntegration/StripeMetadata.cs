namespace SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;

/// <summary>Metadata keys stamped on Stripe objects so their webhooks can be traced back to our records.</summary>
internal static class StripeMetadata
{
    public const string PaymentRequestId = "payment_request_id";
    public const string RefundId = "refund_id";
    public const string UserId = "user_id";

    public static Guid? ReadGuid(IReadOnlyDictionary<string, string>? metadata, string key) =>
        metadata is not null && metadata.TryGetValue(key, out string? value) && Guid.TryParse(value, out Guid id)
            ? id
            : null;
}
