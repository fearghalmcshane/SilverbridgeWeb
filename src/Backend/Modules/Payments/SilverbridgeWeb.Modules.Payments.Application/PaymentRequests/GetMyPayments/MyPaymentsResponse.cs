using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetMyPayments;

public sealed record MyPaymentsResponse(
    IReadOnlyList<PaymentSummaryResponse> Payments,
    IReadOnlyList<SubscriptionSummaryResponse> Subscriptions);

public sealed class PaymentSummaryResponse
{
    public Guid Id { get; init; }

    public string Description { get; init; }

    public string Source { get; init; }

    public PaymentRequestKind Kind { get; init; }

    public string Currency { get; init; }

    public long AmountMinor { get; init; }

    public long AmountRefundedMinor { get; init; }

    public PaymentRequestStatus Status { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }
}

public sealed class SubscriptionSummaryResponse
{
    public Guid Id { get; init; }

    public string Description { get; init; }

    public string Currency { get; init; }

    public long AmountMinor { get; init; }

    public SubscriptionStatus Status { get; init; }

    public DateTime? CurrentPeriodEndUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }
}
