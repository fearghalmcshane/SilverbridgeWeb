using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class SubscriptionPaymentSucceededIntegrationEvent : IntegrationEvent
{
    public SubscriptionPaymentSucceededIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid subscriptionId,
        string source,
        string sourceReference,
        Guid payerUserId,
        long amountMinor,
        DateTime periodStartUtc,
        DateTime periodEndUtc)
        : base(id, occurredOnUtc)
    {
        SubscriptionId = subscriptionId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
        AmountMinor = amountMinor;
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
    }

    public Guid SubscriptionId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }

    public long AmountMinor { get; init; }

    public DateTime PeriodStartUtc { get; init; }

    public DateTime PeriodEndUtc { get; init; }
}
