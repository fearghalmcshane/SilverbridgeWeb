using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class SubscriptionActivatedIntegrationEvent : IntegrationEvent
{
    public SubscriptionActivatedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid subscriptionId,
        string source,
        string sourceReference,
        Guid payerUserId)
        : base(id, occurredOnUtc)
    {
        SubscriptionId = subscriptionId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
    }

    public Guid SubscriptionId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }
}
