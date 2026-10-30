using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

/// <param name="wasReversed">The refund had already been reported as succeeded and has now been undone.</param>
public sealed class PaymentRefundFailedDomainEvent(
    Guid paymentRequestId,
    Guid refundId,
    long amountMinor,
    long totalRefundedMinor,
    bool wasReversed,
    string? failureReason) : DomainEvent
{
    public Guid PaymentRequestId { get; init; } = paymentRequestId;

    public Guid RefundId { get; init; } = refundId;

    public long AmountMinor { get; init; } = amountMinor;

    public long TotalRefundedMinor { get; init; } = totalRefundedMinor;

    public bool WasReversed { get; init; } = wasReversed;

    public string? FailureReason { get; init; } = failureReason;
}
