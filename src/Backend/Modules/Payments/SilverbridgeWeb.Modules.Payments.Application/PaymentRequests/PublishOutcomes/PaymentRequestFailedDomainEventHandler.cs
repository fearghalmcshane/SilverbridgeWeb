using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.PublishOutcomes;

internal sealed class PaymentRequestFailedDomainEventHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IEventBus eventBus)
    : DomainEventHandler<PaymentRequestFailedDomainEvent>
{
    public override async Task Handle(
        PaymentRequestFailedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        PaymentRequest paymentRequest =
            await paymentRequestRepository.GetAsync(domainEvent.PaymentRequestId, cancellationToken) ??
            throw new SilverbridgeWebException(
                nameof(PaymentRequestFailedDomainEventHandler),
                PaymentRequestErrors.NotFound(domainEvent.PaymentRequestId));

        await eventBus.PublishAsync(
            new PaymentFailedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                paymentRequest.Id,
                paymentRequest.Source,
                paymentRequest.SourceReference,
                paymentRequest.PayerUserId,
                paymentRequest.FailureReason),
            cancellationToken);
    }
}
