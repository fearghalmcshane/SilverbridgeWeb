using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Ticketing.Domain.Payments;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Payments.RefundPayment;

internal sealed class PaymentRefundedDomainEventHandler(IPaymentService paymentService)
    : DomainEventHandler<PaymentRefundedDomainEvent>
{
    public override async Task Handle(
        PaymentRefundedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await paymentService.RefundAsync(domainEvent.TransactionId, domainEvent.RefundAmount);
    }
}
