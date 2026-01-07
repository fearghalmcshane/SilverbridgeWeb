using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Ticketing.Domain.Payments;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Payments.RefundPayment;

internal sealed class PaymentPartiallyRefundedDomainEventHandler(IPaymentService paymentService)
    : DomainEventHandler<PaymentPartiallyRefundedDomainEvent>
{
    public override async Task Handle(
        PaymentPartiallyRefundedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await paymentService.RefundAsync(domainEvent.TransactionId, domainEvent.RefundAmount);
    }
}
