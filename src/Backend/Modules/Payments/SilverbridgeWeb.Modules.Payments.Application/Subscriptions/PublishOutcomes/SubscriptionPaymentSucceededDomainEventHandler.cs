using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.PublishOutcomes;

internal sealed class SubscriptionPaymentSucceededDomainEventHandler(
    ISubscriptionRepository subscriptionRepository,
    IEventBus eventBus)
    : DomainEventHandler<SubscriptionPaymentSucceededDomainEvent>
{
    public override async Task Handle(
        SubscriptionPaymentSucceededDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Subscription subscription =
            await subscriptionRepository.GetAsync(domainEvent.SubscriptionId, cancellationToken) ??
            throw new SilverbridgeWebException(
                nameof(SubscriptionPaymentSucceededDomainEventHandler),
                SubscriptionErrors.NotFound(domainEvent.SubscriptionId));

        SubscriptionPayment payment = subscription.Payments.Single(p => p.Id == domainEvent.SubscriptionPaymentId);

        await eventBus.PublishAsync(
            new SubscriptionPaymentSucceededIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                subscription.Id,
                subscription.Source,
                subscription.SourceReference,
                subscription.PayerUserId,
                payment.AmountMinor,
                payment.PeriodStartUtc ?? payment.RecordedAtUtc,
                payment.PeriodEndUtc ?? payment.RecordedAtUtc),
            cancellationToken);
    }
}
