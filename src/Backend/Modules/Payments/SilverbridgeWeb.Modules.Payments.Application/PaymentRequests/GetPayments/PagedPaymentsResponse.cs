using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPayments;

public sealed record PagedPaymentsResponse(
    IReadOnlyList<AdminPaymentSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed class AdminPaymentSummaryResponse
{
    public Guid Id { get; init; }

    public Guid PayerUserId { get; init; }

    public string? PayerName { get; init; }

    public string? PayerEmail { get; init; }

    public string Description { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public PaymentRequestKind Kind { get; init; }

    public string Currency { get; init; }

    public long AmountMinor { get; init; }

    public long AmountRefundedMinor { get; init; }

    public PaymentRequestStatus Status { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }
}
