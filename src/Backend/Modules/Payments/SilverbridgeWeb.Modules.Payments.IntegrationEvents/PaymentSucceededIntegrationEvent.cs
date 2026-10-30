using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class PaymentSucceededIntegrationEvent : IntegrationEvent
{
    public PaymentSucceededIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentRequestId,
        string source,
        string sourceReference,
        Guid payerUserId,
        long amountMinor,
        string currency)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
        AmountMinor = amountMinor;
        Currency = currency;
    }

    public Guid PaymentRequestId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }

    public long AmountMinor { get; init; }

    public string Currency { get; init; }
}
