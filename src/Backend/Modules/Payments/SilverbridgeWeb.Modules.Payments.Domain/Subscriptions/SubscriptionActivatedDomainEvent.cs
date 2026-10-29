using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

public sealed class SubscriptionActivatedDomainEvent(Guid subscriptionId) : DomainEvent
{
    public Guid SubscriptionId { get; init; } = subscriptionId;
}
