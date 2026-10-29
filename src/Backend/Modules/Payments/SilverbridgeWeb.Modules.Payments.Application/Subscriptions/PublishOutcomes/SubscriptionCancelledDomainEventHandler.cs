using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.PublishOutcomes;

internal sealed class SubscriptionCancelledDomainEventHandler(
    ISubscriptionRepository subscriptionRepository,
    IEventBus eventBus)
    : DomainEventHandler<SubscriptionCancelledDomainEvent>
{
    public override async Task Handle(
        SubscriptionCancelledDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Subscription subscription =
            await subscriptionRepository.GetAsync(domainEvent.SubscriptionId, cancellationToken) ??
            throw new SilverbridgeWebException(
                nameof(SubscriptionCancelledDomainEventHandler),
                SubscriptionErrors.NotFound(domainEvent.SubscriptionId));

        await eventBus.PublishAsync(
            new SubscriptionCancelledIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                subscription.Id,
                subscription.Source,
                subscription.SourceReference,
                subscription.PayerUserId),
            cancellationToken);
    }
}
