using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class PaymentFailedIntegrationEvent : IntegrationEvent
{
    public PaymentFailedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentRequestId,
        string source,
        string sourceReference,
        Guid payerUserId,
        string? reason)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
        Reason = reason;
    }

    public Guid PaymentRequestId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }

    public string? Reason { get; init; }
}
