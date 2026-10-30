using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.PublishOutcomes;

internal sealed class SubscriptionActivatedDomainEventHandler(
    ISubscriptionRepository subscriptionRepository,
    IEventBus eventBus)
    : DomainEventHandler<SubscriptionActivatedDomainEvent>
{
    public override async Task Handle(
        SubscriptionActivatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Subscription subscription =
            await subscriptionRepository.GetAsync(domainEvent.SubscriptionId, cancellationToken) ??
            throw new SilverbridgeWebException(
                nameof(SubscriptionActivatedDomainEventHandler),
                SubscriptionErrors.NotFound(domainEvent.SubscriptionId));

        await eventBus.PublishAsync(
            new SubscriptionActivatedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                subscription.Id,
                subscription.Source,
                subscription.SourceReference,
                subscription.PayerUserId),
            cancellationToken);
    }
}
