using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.PublishOutcomes;

internal sealed class SubscriptionPaymentFailedDomainEventHandler(
    ISubscriptionRepository subscriptionRepository,
    IEventBus eventBus)
    : DomainEventHandler<SubscriptionPaymentFailedDomainEvent>
{
    public override async Task Handle(
        SubscriptionPaymentFailedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Subscription subscription =
            await subscriptionRepository.GetAsync(domainEvent.SubscriptionId, cancellationToken) ??
            throw new SilverbridgeWebException(
                nameof(SubscriptionPaymentFailedDomainEventHandler),
                SubscriptionErrors.NotFound(domainEvent.SubscriptionId));

        SubscriptionPayment payment = subscription.Payments.Single(p => p.Id == domainEvent.SubscriptionPaymentId);

        await eventBus.PublishAsync(
            new SubscriptionPaymentFailedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                subscription.Id,
                subscription.Source,
                subscription.SourceReference,
                subscription.PayerUserId,
                payment.AmountMinor,
                payment.FailureReason),
            cancellationToken);
    }
}
