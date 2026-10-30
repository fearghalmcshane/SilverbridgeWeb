using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

public sealed class SubscriptionPayment : Entity
{
    private SubscriptionPayment()
    {
    }

    public Guid Id { get; private set; }

    public Guid SubscriptionId { get; private set; }

    public string ProviderInvoiceId { get; private set; }

    public long AmountMinor { get; private set; }

    public SubscriptionPaymentStatus Status { get; private set; }

    public DateTime? PeriodStartUtc { get; private set; }

    public DateTime? PeriodEndUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }

    internal static SubscriptionPayment Create(
        Guid subscriptionId,
        string providerInvoiceId,
        long amountMinor,
        SubscriptionPaymentStatus status,
        DateTime? periodStartUtc,
        DateTime? periodEndUtc,
        string? failureReason,
        DateTime utcNow)
    {
        return new SubscriptionPayment
        {
            Id = Ulid.NewUlid().ToGuid(),
            SubscriptionId = subscriptionId,
            ProviderInvoiceId = providerInvoiceId,
            AmountMinor = amountMinor,
            Status = status,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc,
            FailureReason = failureReason,
            RecordedAtUtc = utcNow
        };
    }

    internal void MarkSucceeded(long amountMinor, DateTime? periodStartUtc, DateTime? periodEndUtc, DateTime utcNow)
    {
        Status = SubscriptionPaymentStatus.Succeeded;
        AmountMinor = amountMinor;
        PeriodStartUtc = periodStartUtc ?? PeriodStartUtc;
        PeriodEndUtc = periodEndUtc ?? PeriodEndUtc;
        FailureReason = null;
        RecordedAtUtc = utcNow;
    }
}
