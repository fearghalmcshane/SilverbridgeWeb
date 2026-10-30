using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

/// <summary>
/// A refund did not reach the payer. When <see cref="WasReversed"/> is set, a <see cref="PaymentRefundedIntegrationEvent"/>
/// was published for it earlier and should be treated as undone.
/// </summary>
public sealed class PaymentRefundFailedIntegrationEvent : IntegrationEvent
{
    public PaymentRefundFailedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentRequestId,
        string source,
        string sourceReference,
        Guid payerUserId,
        Guid refundId,
        long amountMinor,
        long totalRefundedMinor,
        bool wasReversed,
        string? failureReason)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
        RefundId = refundId;
        AmountMinor = amountMinor;
        TotalRefundedMinor = totalRefundedMinor;
        WasReversed = wasReversed;
        FailureReason = failureReason;
    }

    public Guid PaymentRequestId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }

    public Guid RefundId { get; init; }

    public long AmountMinor { get; init; }

    public long TotalRefundedMinor { get; init; }

    public bool WasReversed { get; init; }

    public string? FailureReason { get; init; }
}
