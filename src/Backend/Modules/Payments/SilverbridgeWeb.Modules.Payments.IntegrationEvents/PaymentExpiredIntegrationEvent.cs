using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class PaymentExpiredIntegrationEvent : IntegrationEvent
{
    public PaymentExpiredIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentRequestId,
        string source,
        string sourceReference,
        Guid payerUserId)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
    }

    public Guid PaymentRequestId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }
}
