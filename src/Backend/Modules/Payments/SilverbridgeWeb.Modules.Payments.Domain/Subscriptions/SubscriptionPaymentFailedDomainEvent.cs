using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

/// <summary>
/// Carries the failed attempt's amount and reason, because the payment record changes if a later retry succeeds
/// before this event is published.
/// </summary>
public sealed class SubscriptionPaymentFailedDomainEvent(
    Guid subscriptionId,
    Guid subscriptionPaymentId,
    long amountMinor,
    string? failureReason) : DomainEvent
{
    public Guid SubscriptionId { get; init; } = subscriptionId;

    public Guid SubscriptionPaymentId { get; init; } = subscriptionPaymentId;

    public long AmountMinor { get; init; } = amountMinor;

    public string? FailureReason { get; init; } = failureReason;
}
