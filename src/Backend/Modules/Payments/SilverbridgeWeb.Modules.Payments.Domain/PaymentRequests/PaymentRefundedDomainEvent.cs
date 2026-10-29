using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public sealed class PaymentRefundedDomainEvent(
    Guid paymentRequestId,
    Guid refundId,
    long amountRefundedMinor,
    long totalRefundedMinor,
    bool isFullyRefunded) : DomainEvent
{
    public Guid PaymentRequestId { get; init; } = paymentRequestId;

    public Guid RefundId { get; init; } = refundId;

    public long AmountRefundedMinor { get; init; } = amountRefundedMinor;

    public long TotalRefundedMinor { get; init; } = totalRefundedMinor;

    public bool IsFullyRefunded { get; init; } = isFullyRefunded;
}
