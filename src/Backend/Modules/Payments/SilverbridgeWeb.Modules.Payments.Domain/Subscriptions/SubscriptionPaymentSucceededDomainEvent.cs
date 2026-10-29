using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

public sealed class SubscriptionPaymentSucceededDomainEvent(Guid subscriptionId, Guid subscriptionPaymentId) : DomainEvent
{
    public Guid SubscriptionId { get; init; } = subscriptionId;

    public Guid SubscriptionPaymentId { get; init; } = subscriptionPaymentId;
}
