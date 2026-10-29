using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class PaymentRefundedIntegrationEvent : IntegrationEvent
{
    public PaymentRefundedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentRequestId,
        string source,
        string sourceReference,
        Guid payerUserId,
        long amountRefundedMinor,
        long totalRefundedMinor,
        bool isFullyRefunded)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
        AmountRefundedMinor = amountRefundedMinor;
        TotalRefundedMinor = totalRefundedMinor;
        IsFullyRefunded = isFullyRefunded;
    }

    public Guid PaymentRequestId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }

    public long AmountRefundedMinor { get; init; }

    public long TotalRefundedMinor { get; init; }

    public bool IsFullyRefunded { get; init; }
}
