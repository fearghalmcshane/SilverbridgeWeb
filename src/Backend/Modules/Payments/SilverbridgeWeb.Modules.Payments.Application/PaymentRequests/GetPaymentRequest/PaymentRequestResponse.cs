using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPaymentRequest;

public sealed class PaymentRequestResponse
{
    public Guid Id { get; init; }

    public Guid PayerUserId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public string Description { get; init; }

    public PaymentRequestKind Kind { get; init; }

    public string Currency { get; init; }

    public long AmountMinor { get; init; }

    public long AmountRefundedMinor { get; init; }

    public PaymentRequestStatus Status { get; init; }

    public string? FailureReason { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public List<PaymentRequestLineResponse> Lines { get; init; } = [];

    public List<RefundResponse> Refunds { get; init; } = [];
}

public sealed class PaymentRequestLineResponse
{
    public string Description { get; init; }

    public int Quantity { get; init; }

    public long UnitAmountMinor { get; init; }
}

public sealed class RefundResponse
{
    public Guid Id { get; init; }

    public long AmountMinor { get; init; }

    public string Reason { get; init; }

    public RefundStatus Status { get; init; }

    public string? FailureReason { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }
}
