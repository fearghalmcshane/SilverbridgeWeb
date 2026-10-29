using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

/// <summary>
/// A provider callback translated into provider-neutral terms, so no provider SDK types reach the application layer.
/// </summary>
public abstract record ProviderEvent(string ProviderEventId);

public sealed record CheckoutCompletedProviderEvent(
    string ProviderEventId,
    Guid PaymentRequestId,
    string SessionId,
    bool IsPaid,
    string? PaymentIntentId,
    string? CustomerId,
    string? SubscriptionId) : ProviderEvent(ProviderEventId);

public sealed record AsyncPaymentSucceededProviderEvent(
    string ProviderEventId,
    Guid PaymentRequestId,
    string SessionId,
    string? PaymentIntentId) : ProviderEvent(ProviderEventId);

public sealed record AsyncPaymentFailedProviderEvent(
    string ProviderEventId,
    Guid PaymentRequestId,
    string SessionId) : ProviderEvent(ProviderEventId);

public sealed record CheckoutExpiredProviderEvent(
    string ProviderEventId,
    Guid PaymentRequestId,
    string SessionId) : ProviderEvent(ProviderEventId);

public sealed record RefundUpdatedProviderEvent(
    string ProviderEventId,
    string ProviderRefundId,
    Guid? PaymentRequestId,
    Guid? RefundId,
    string? PaymentIntentId,
    long AmountMinor,
    RefundStatus Status,
    string? FailureReason) : ProviderEvent(ProviderEventId);

public sealed record SubscriptionUpdatedProviderEvent(
    string ProviderEventId,
    string ProviderSubscriptionId,
    Guid? PaymentRequestId,
    string? CustomerId,
    SubscriptionStatus Status,
    DateTime? CurrentPeriodEndUtc) : ProviderEvent(ProviderEventId);

public sealed record InvoicePaymentProviderEvent(
    string ProviderEventId,
    string ProviderInvoiceId,
    string ProviderSubscriptionId,
    long AmountMinor,
    bool Succeeded,
    DateTime? PeriodStartUtc,
    DateTime? PeriodEndUtc,
    string? FailureReason) : ProviderEvent(ProviderEventId);
