using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

/// <summary>
/// An ongoing monthly payment started from a <see cref="PaymentRequest"/> of kind
/// <see cref="PaymentRequestKind.MonthlySubscription"/>. It shares that request's identifier, so the
/// requesting module can refer to it without a lookup.
/// </summary>
public sealed class Subscription : Entity
{
    private readonly List<SubscriptionPayment> _payments = [];

    private Subscription()
    {
    }

    public Guid Id { get; private set; }

    public Guid PayerUserId { get; private set; }

    public string Source { get; private set; }

    public string SourceReference { get; private set; }

    public string Description { get; private set; }

    public long AmountMinor { get; private set; }

    public string Currency { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public string ProviderSubscriptionId { get; private set; }

    public string? ProviderCustomerId { get; private set; }

    public DateTime? CurrentPeriodEndUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ActivatedAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public IReadOnlyCollection<SubscriptionPayment> Payments => _payments.ToList();

    public static Result<Subscription> Start(
        PaymentRequest paymentRequest,
        string providerSubscriptionId,
        string? providerCustomerId,
        DateTime utcNow)
    {
        if (paymentRequest.Kind != PaymentRequestKind.MonthlySubscription)
        {
            return Result.Failure<Subscription>(SubscriptionErrors.NotASubscription);
        }

        return new Subscription
        {
            Id = paymentRequest.Id,
            PayerUserId = paymentRequest.PayerUserId,
            Source = paymentRequest.Source,
            SourceReference = paymentRequest.SourceReference,
            Description = paymentRequest.Description,
            AmountMinor = paymentRequest.AmountMinor,
            Currency = paymentRequest.Currency,
            Status = SubscriptionStatus.Incomplete,
            ProviderSubscriptionId = providerSubscriptionId,
            ProviderCustomerId = providerCustomerId,
            CreatedAtUtc = utcNow
        };
    }

    public void UpdateStatus(SubscriptionStatus status, DateTime? currentPeriodEndUtc, DateTime utcNow)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            return;
        }

        CurrentPeriodEndUtc = currentPeriodEndUtc ?? CurrentPeriodEndUtc;

        if (status == Status)
        {
            return;
        }

        Status = status;

        if (status == SubscriptionStatus.Active && ActivatedAtUtc is null)
        {
            ActivatedAtUtc = utcNow;
            Raise(new SubscriptionActivatedDomainEvent(Id));
        }

        if (status == SubscriptionStatus.Cancelled)
        {
            CancelledAtUtc = utcNow;
            Raise(new SubscriptionCancelledDomainEvent(Id));
        }
    }

    public void RecordPayment(
        string providerInvoiceId,
        long amountMinor,
        SubscriptionPaymentStatus status,
        DateTime? periodStartUtc,
        DateTime? periodEndUtc,
        string? failureReason,
        DateTime utcNow)
    {
        SubscriptionPayment? existing = _payments.SingleOrDefault(p => p.ProviderInvoiceId == providerInvoiceId);

        if (existing is null)
        {
            var payment = SubscriptionPayment.Create(
                Id, providerInvoiceId, amountMinor, status, periodStartUtc, periodEndUtc, failureReason, utcNow);
            _payments.Add(payment);
            RaisePaymentEvent(payment);
            return;
        }

        // A failed invoice can be paid on a later retry; anything else is a repeat of what is already recorded.
        if (existing.Status == SubscriptionPaymentStatus.Failed && status == SubscriptionPaymentStatus.Succeeded)
        {
            existing.MarkSucceeded(amountMinor, periodStartUtc, periodEndUtc, utcNow);
            RaisePaymentEvent(existing);
        }
    }

    private void RaisePaymentEvent(SubscriptionPayment payment)
    {
        if (payment.Status == SubscriptionPaymentStatus.Succeeded)
        {
            Raise(new SubscriptionPaymentSucceededDomainEvent(Id, payment.Id));
        }
        else
        {
            Raise(new SubscriptionPaymentFailedDomainEvent(Id, payment.Id));
        }
    }
}
