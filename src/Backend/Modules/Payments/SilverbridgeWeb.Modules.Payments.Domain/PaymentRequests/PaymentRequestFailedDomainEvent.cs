using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public sealed class PaymentRequestFailedDomainEvent(Guid paymentRequestId) : DomainEvent
{
    public Guid PaymentRequestId { get; init; } = paymentRequestId;
}
