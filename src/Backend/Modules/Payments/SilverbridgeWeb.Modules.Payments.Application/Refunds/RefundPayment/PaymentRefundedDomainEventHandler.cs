using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;

internal sealed class PaymentRefundedDomainEventHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IEventBus eventBus)
    : DomainEventHandler<PaymentRefundedDomainEvent>
{
    public override async Task Handle(
        PaymentRefundedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        PaymentRequest paymentRequest =
            await paymentRequestRepository.GetAsync(domainEvent.PaymentRequestId, cancellationToken) ??
            throw new SilverbridgeWebException(
                nameof(PaymentRefundedDomainEventHandler),
                PaymentRequestErrors.NotFound(domainEvent.PaymentRequestId));

        await eventBus.PublishAsync(
            new PaymentRefundedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                paymentRequest.Id,
                paymentRequest.Source,
                paymentRequest.SourceReference,
                paymentRequest.PayerUserId,
                domainEvent.AmountRefundedMinor,
                domainEvent.TotalRefundedMinor,
                domainEvent.IsFullyRefunded),
            cancellationToken);
    }
}
