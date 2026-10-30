using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;

internal sealed class PaymentRefundFailedDomainEventHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IEventBus eventBus)
    : DomainEventHandler<PaymentRefundFailedDomainEvent>
{
    public override async Task Handle(
        PaymentRefundFailedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        PaymentRequest paymentRequest =
            await paymentRequestRepository.GetAsync(domainEvent.PaymentRequestId, cancellationToken) ??
            throw new SilverbridgeWebException(
                nameof(PaymentRefundFailedDomainEventHandler),
                PaymentRequestErrors.NotFound(domainEvent.PaymentRequestId));

        await eventBus.PublishAsync(
            new PaymentRefundFailedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                paymentRequest.Id,
                paymentRequest.Source,
                paymentRequest.SourceReference,
                paymentRequest.PayerUserId,
                domainEvent.RefundId,
                domainEvent.AmountMinor,
                domainEvent.TotalRefundedMinor,
                domainEvent.WasReversed,
                domainEvent.FailureReason),
            cancellationToken);
    }
}
