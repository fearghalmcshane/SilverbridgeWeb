using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class PaymentRequestCancelledIntegrationEvent : IntegrationEvent
{
    public PaymentRequestCancelledIntegrationEvent(Guid id, DateTime occurredOnUtc, Guid paymentRequestId)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
    }

    public Guid PaymentRequestId { get; init; }
}
