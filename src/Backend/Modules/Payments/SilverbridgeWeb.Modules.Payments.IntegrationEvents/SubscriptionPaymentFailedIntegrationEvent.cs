using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class SubscriptionPaymentFailedIntegrationEvent : IntegrationEvent
{
    public SubscriptionPaymentFailedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid subscriptionId,
        string source,
        string sourceReference,
        Guid payerUserId,
        long amountMinor,
        string? reason)
        : base(id, occurredOnUtc)
    {
        SubscriptionId = subscriptionId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
        AmountMinor = amountMinor;
        Reason = reason;
    }

    public Guid SubscriptionId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }

    public long AmountMinor { get; init; }

    public string? Reason { get; init; }
}
