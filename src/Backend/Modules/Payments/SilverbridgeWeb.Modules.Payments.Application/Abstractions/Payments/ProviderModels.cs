using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

public sealed record ProviderCustomerRequest(Guid UserId, string Email, string Name);

public sealed record CheckoutLine(string Description, int Quantity, long UnitAmountMinor);

public sealed record CheckoutSessionRequest(
    Guid PaymentRequestId,
    PaymentRequestKind Kind,
    string Description,
    string Currency,
    IReadOnlyList<CheckoutLine> Lines,
    string? ProviderCustomerId,
    DateTime ExpiresAtUtc);

public sealed record CheckoutSession(string SessionId, Uri Url, DateTime ExpiresAtUtc);

public enum CheckoutSessionState
{
    Open = 0,
    Complete = 1,
    Expired = 2
}

/// <param name="PaymentFailed">
/// The checkout completed but its payment did not go through, for example a Direct Debit the bank refused.
/// A completed checkout that is neither paid nor failed is still being collected.
/// </param>
/// <param name="SubscriptionStatus">The monthly payment's current status, when the checkout started one.</param>
public sealed record CheckoutSessionSnapshot(
    string SessionId,
    CheckoutSessionState State,
    bool IsPaid,
    string? PaymentIntentId,
    string? CustomerId,
    string? SubscriptionId,
    bool PaymentFailed = false,
    string? FailureReason = null,
    SubscriptionStatus? SubscriptionStatus = null,
    DateTime? SubscriptionPeriodEndUtc = null);

public sealed record ProviderRefundRequest(
    Guid RefundId,
    Guid PaymentRequestId,
    string PaymentIntentId,
    long AmountMinor);

public sealed record ProviderRefund(string ProviderRefundId, RefundStatus Status, string? FailureReason);
