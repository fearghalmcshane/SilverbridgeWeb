using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

/// <summary>
/// A request from another module to collect money from a user, and what happened to it.
/// Provider callbacks can arrive more than once and out of order, so transitions that have
/// already happened succeed without changing anything or raising events again.
/// </summary>
public sealed class PaymentRequest : Entity
{
    public const int SourceMaxLength = 50;
    public const int SourceReferenceMaxLength = 200;
    public const int DescriptionMaxLength = 200;
    public const int ReasonMaxLength = 500;

    private static readonly TimeSpan OpenCheckoutMinimumRemaining = TimeSpan.FromMinutes(2);

    private readonly List<PaymentRequestLine> _lines = [];
    private readonly List<Refund> _refunds = [];

    private PaymentRequest()
    {
    }

    public Guid Id { get; private set; }

    public Guid PayerUserId { get; private set; }

    public string Source { get; private set; }

    public string SourceReference { get; private set; }

    public string Description { get; private set; }

    public PaymentRequestKind Kind { get; private set; }

    public string Currency { get; private set; }

    public long AmountMinor { get; private set; }

    public long AmountRefundedMinor { get; private set; }

    public PaymentRequestStatus Status { get; private set; }

    public string? ProviderCheckoutSessionId { get; private set; }

    public string? CheckoutUrl { get; private set; }

    public DateTime? CheckoutExpiresAtUtc { get; private set; }

    public string? ProviderPaymentIntentId { get; private set; }

    public string? ProviderCustomerId { get; private set; }

    public string? ProviderSubscriptionId { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<PaymentRequestLine> Lines => _lines.ToList();

    public IReadOnlyCollection<Refund> Refunds => _refunds.ToList();

    public long RefundableAmountMinor =>
        AmountMinor - AmountRefundedMinor - _refunds.Where(r => r.Status == RefundStatus.Pending).Sum(r => r.AmountMinor);

    public static Result<PaymentRequest> Create(
        Guid id,
        Guid payerUserId,
        string source,
        string sourceReference,
        string description,
        string currency,
        PaymentRequestKind kind,
        IReadOnlyCollection<PaymentRequestLineDetails> lines,
        DateTime utcNow)
    {
        if (!Currencies.IsSupported(currency))
        {
            return Result.Failure<PaymentRequest>(PaymentRequestErrors.UnsupportedCurrency);
        }

        if (lines.Count == 0)
        {
            return Result.Failure<PaymentRequest>(PaymentRequestErrors.NoLines);
        }

        if (lines.Any(l => string.IsNullOrWhiteSpace(l.Description) || l.Quantity <= 0 || l.UnitAmountMinor <= 0))
        {
            return Result.Failure<PaymentRequest>(PaymentRequestErrors.InvalidLine);
        }

        var paymentRequest = new PaymentRequest
        {
            Id = id,
            PayerUserId = payerUserId,
            Source = source,
            SourceReference = sourceReference,
            Description = description,
            Kind = kind,
            Currency = currency,
            Status = PaymentRequestStatus.Pending,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        foreach (PaymentRequestLineDetails line in lines)
        {
            paymentRequest._lines.Add(
                PaymentRequestLine.Create(id, line.Description, line.Quantity, line.UnitAmountMinor));
        }

        paymentRequest.AmountMinor = paymentRequest._lines.Sum(l => l.AmountMinor);

        return paymentRequest;
    }

    public bool HasOpenCheckout(DateTime utcNow) =>
        Status == PaymentRequestStatus.AwaitingPayment &&
        CheckoutUrl is not null &&
        CheckoutExpiresAtUtc > utcNow.Add(OpenCheckoutMinimumRemaining);

    public Result AttachCheckout(
        string sessionId,
        Uri checkoutUrl,
        DateTime expiresAtUtc,
        string? providerCustomerId,
        DateTime utcNow)
    {
        if (Status is not (PaymentRequestStatus.Pending or PaymentRequestStatus.AwaitingPayment))
        {
            return Result.Failure(PaymentRequestErrors.NotPayable);
        }

        ProviderCheckoutSessionId = sessionId;
        CheckoutUrl = checkoutUrl.ToString();
        CheckoutExpiresAtUtc = expiresAtUtc;
        ProviderCustomerId = providerCustomerId ?? ProviderCustomerId;
        Status = PaymentRequestStatus.AwaitingPayment;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// The payer finished checkout. Card payments are paid at this point; Direct Debit payments
    /// are still being collected and finish later through <see cref="MarkAsyncPaymentSucceeded"/>.
    /// </summary>
    public Result MarkCheckoutCompleted(
        string sessionId,
        bool isPaid,
        string? paymentIntentId,
        string? providerCustomerId,
        string? providerSubscriptionId,
        DateTime utcNow)
    {
        if (Status is PaymentRequestStatus.Succeeded or PaymentRequestStatus.Refunded)
        {
            return Result.Success();
        }

        if (Status == PaymentRequestStatus.Processing && !isPaid)
        {
            return Result.Success();
        }

        if (Status is not (PaymentRequestStatus.AwaitingPayment or PaymentRequestStatus.Processing))
        {
            return Result.Failure(PaymentRequestErrors.InvalidTransition(Status, "completed"));
        }

        // Money may have been taken on a session that was later replaced, so record whichever session completed.
        ProviderCheckoutSessionId = sessionId;
        ProviderPaymentIntentId = paymentIntentId ?? ProviderPaymentIntentId;
        ProviderCustomerId = providerCustomerId ?? ProviderCustomerId;
        ProviderSubscriptionId = providerSubscriptionId ?? ProviderSubscriptionId;
        CheckoutUrl = null;
        UpdatedAtUtc = utcNow;

        if (isPaid)
        {
            Succeed(utcNow);
        }
        else
        {
            Status = PaymentRequestStatus.Processing;
        }

        return Result.Success();
    }

    public Result MarkAsyncPaymentSucceeded(string? paymentIntentId, DateTime utcNow)
    {
        if (Status is PaymentRequestStatus.Succeeded or PaymentRequestStatus.Refunded)
        {
            return Result.Success();
        }

        if (Status is not (PaymentRequestStatus.AwaitingPayment or PaymentRequestStatus.Processing))
        {
            return Result.Failure(PaymentRequestErrors.InvalidTransition(Status, "marked as paid"));
        }

        ProviderPaymentIntentId = paymentIntentId ?? ProviderPaymentIntentId;
        CheckoutUrl = null;
        UpdatedAtUtc = utcNow;
        Succeed(utcNow);

        return Result.Success();
    }

    public Result MarkAsyncPaymentFailed(string? reason, DateTime utcNow)
    {
        if (Status == PaymentRequestStatus.Failed)
        {
            return Result.Success();
        }

        if (Status is not (PaymentRequestStatus.AwaitingPayment or PaymentRequestStatus.Processing))
        {
            return Result.Failure(PaymentRequestErrors.InvalidTransition(Status, "marked as failed"));
        }

        Status = PaymentRequestStatus.Failed;
        FailureReason = Truncate(reason);
        CheckoutUrl = null;
        CompletedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        Raise(new PaymentRequestFailedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// The checkout session expired without payment. Expiry of a session that has since been replaced,
    /// or that arrives after the request settled, is ignored.
    /// </summary>
    public Result MarkCheckoutExpired(string sessionId, DateTime utcNow)
    {
        if (Status != PaymentRequestStatus.AwaitingPayment || sessionId != ProviderCheckoutSessionId)
        {
            return Result.Success();
        }

        Status = PaymentRequestStatus.Expired;
        CheckoutUrl = null;
        CompletedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        Raise(new PaymentRequestExpiredDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status == PaymentRequestStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status is not (PaymentRequestStatus.Pending or PaymentRequestStatus.AwaitingPayment))
        {
            return Result.Failure(PaymentRequestErrors.InvalidTransition(Status, "cancelled"));
        }

        Status = PaymentRequestStatus.Cancelled;
        CheckoutUrl = null;
        CompletedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    public Result<Refund> RequestRefund(long? amountMinor, string reason, Guid? requestedByUserId, DateTime utcNow)
    {
        if (Kind != PaymentRequestKind.OneOff ||
            Status != PaymentRequestStatus.Succeeded ||
            ProviderPaymentIntentId is null)
        {
            return Result.Failure<Refund>(PaymentRequestErrors.NotRefundable);
        }

        long amount = amountMinor ?? RefundableAmountMinor;
        if (amount <= 0 || amount > RefundableAmountMinor)
        {
            return Result.Failure<Refund>(PaymentRequestErrors.InvalidRefundAmount);
        }

        var refund = Refund.Create(Id, amount, Truncate(reason) ?? string.Empty, requestedByUserId, null, utcNow);
        _refunds.Add(refund);
        UpdatedAtUtc = utcNow;

        return refund;
    }

    public Result AttachProviderRefund(Guid refundId, string providerRefundId)
    {
        Refund? refund = _refunds.SingleOrDefault(r => r.Id == refundId);
        if (refund is null)
        {
            return Result.Failure(PaymentRequestErrors.RefundNotFound(refundId));
        }

        refund.AttachProviderRefund(providerRefundId);

        return Result.Success();
    }

    /// <summary>
    /// Applies a refund status reported by the provider. Refunds made outside this site, for example in
    /// the provider's dashboard, are recorded the first time they are seen.
    /// </summary>
    public Result ApplyRefundUpdate(
        string providerRefundId,
        Guid? refundId,
        long amountMinor,
        RefundStatus status,
        string? failureReason,
        DateTime utcNow)
    {
        Refund? refund =
            _refunds.SingleOrDefault(r => r.ProviderRefundId == providerRefundId) ??
            (refundId is null ? null : _refunds.SingleOrDefault(r => r.Id == refundId));

        if (refund is null)
        {
            if (status == RefundStatus.Failed)
            {
                return Result.Success();
            }

            if (Status is not (PaymentRequestStatus.Succeeded or PaymentRequestStatus.Refunded))
            {
                return Result.Failure(PaymentRequestErrors.NotRefundable);
            }

            refund = Refund.Create(Id, amountMinor, "Refunded through the payment provider", null, providerRefundId, utcNow);
            _refunds.Add(refund);
        }
        else if (refund.ProviderRefundId is null)
        {
            refund.AttachProviderRefund(providerRefundId);
        }

        if (status == RefundStatus.Succeeded && refund.MarkSucceeded(utcNow))
        {
            AmountRefundedMinor += refund.AmountMinor;
            bool isFullyRefunded = AmountRefundedMinor >= AmountMinor;
            if (isFullyRefunded)
            {
                Status = PaymentRequestStatus.Refunded;
            }

            UpdatedAtUtc = utcNow;

            Raise(new PaymentRefundedDomainEvent(Id, refund.Id, refund.AmountMinor, AmountRefundedMinor, isFullyRefunded));
        }
        else if (status == RefundStatus.Failed && refund.MarkFailed(Truncate(failureReason), utcNow))
        {
            UpdatedAtUtc = utcNow;
        }

        return Result.Success();
    }

    /// <summary>Records that the provider was asked about this request, so reconciliation waits before asking again.</summary>
    public void MarkReconciliationChecked(DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
    }

    private void Succeed(DateTime utcNow)
    {
        Status = PaymentRequestStatus.Succeeded;
        FailureReason = null;
        CompletedAtUtc = utcNow;

        Raise(new PaymentRequestSucceededDomainEvent(Id));
    }

    private static string? Truncate(string? value) =>
        value is { Length: > ReasonMaxLength } ? value[..ReasonMaxLength] : value;
}
