using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class SubscriptionCancellationRequestedIntegrationEvent : IntegrationEvent
{
    public SubscriptionCancellationRequestedIntegrationEvent(Guid id, DateTime occurredOnUtc, Guid subscriptionId)
        : base(id, occurredOnUtc)
    {
        SubscriptionId = subscriptionId;
    }

    /// <summary>The subscription identifier, which is the identifier of the payment request that started it.</summary>
    public Guid SubscriptionId { get; init; }
}
