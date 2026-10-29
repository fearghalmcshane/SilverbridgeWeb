using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public sealed class Refund : Entity
{
    private Refund()
    {
    }

    public Guid Id { get; private set; }

    public Guid PaymentRequestId { get; private set; }

    public long AmountMinor { get; private set; }

    public string Reason { get; private set; }

    public RefundStatus Status { get; private set; }

    public string? ProviderRefundId { get; private set; }

    public Guid? RequestedByUserId { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    internal static Refund Create(
        Guid paymentRequestId,
        long amountMinor,
        string reason,
        Guid? requestedByUserId,
        string? providerRefundId,
        DateTime utcNow)
    {
        return new Refund
        {
            Id = Ulid.NewUlid().ToGuid(),
            PaymentRequestId = paymentRequestId,
            AmountMinor = amountMinor,
            Reason = reason,
            Status = RefundStatus.Pending,
            RequestedByUserId = requestedByUserId,
            ProviderRefundId = providerRefundId,
            CreatedAtUtc = utcNow
        };
    }

    internal void AttachProviderRefund(string providerRefundId)
    {
        ProviderRefundId = providerRefundId;
    }

    internal bool MarkSucceeded(DateTime utcNow)
    {
        if (Status != RefundStatus.Pending)
        {
            return false;
        }

        Status = RefundStatus.Succeeded;
        CompletedAtUtc = utcNow;
        return true;
    }

    internal bool MarkFailed(string? failureReason, DateTime utcNow)
    {
        if (Status != RefundStatus.Pending)
        {
            return false;
        }

        Status = RefundStatus.Failed;
        FailureReason = failureReason;
        CompletedAtUtc = utcNow;
        return true;
    }
}
