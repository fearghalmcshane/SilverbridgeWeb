using Microsoft.Extensions.Logging;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Application.Checkout.CompleteCheckout;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Reconciliation.ReconcilePendingPayments;

/// <summary>
/// Catches payments whose webhook never arrived: any checkout that should have finished by now is looked up
/// at the provider and its real state applied.
/// </summary>
internal sealed class ReconcilePendingPaymentsCommandHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IPaymentProvider paymentProvider,
    CheckoutCompletion checkoutCompletion,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<ReconcilePendingPaymentsCommandHandler> logger)
    : ICommandHandler<ReconcilePendingPaymentsCommand, int>
{
    private static readonly TimeSpan WebhookGracePeriod = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ProcessingCheckInterval = TimeSpan.FromHours(6);

    public async Task<Result<int>> Handle(ReconcilePendingPaymentsCommand request, CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;

        IReadOnlyList<PaymentRequest> unsettled = await paymentRequestRepository.GetUnsettledAsync(
            utcNow - WebhookGracePeriod,
            utcNow - ProcessingCheckInterval,
            request.BatchSize,
            cancellationToken);

        foreach (PaymentRequest paymentRequest in unsettled)
        {
            Result<CheckoutSessionSnapshot> snapshot = await paymentProvider.GetCheckoutSessionAsync(
                paymentRequest.ProviderCheckoutSessionId!,
                cancellationToken);

            if (snapshot.IsFailure)
            {
                logger.LogWarning(
                    "Could not reconcile payment request {PaymentRequestId}: {Error}",
                    paymentRequest.Id,
                    snapshot.Error.Description);
                continue;
            }

            paymentRequest.MarkReconciliationChecked(utcNow);

            Result result = await ApplyAsync(paymentRequest, snapshot.Value, utcNow, cancellationToken);
            if (result.IsFailure)
            {
                logger.LogWarning(
                    "Reconciling payment request {PaymentRequestId} was rejected: {Error}",
                    paymentRequest.Id,
                    result.Error.Description);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return unsettled.Count;
    }

    private async Task<Result> ApplyAsync(
        PaymentRequest paymentRequest,
        CheckoutSessionSnapshot snapshot,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (snapshot.State == CheckoutSessionState.Expired)
        {
            return paymentRequest.MarkCheckoutExpired(snapshot.SessionId, utcNow);
        }

        if (snapshot.State != CheckoutSessionState.Complete)
        {
            return Result.Success();
        }

        if (snapshot.PaymentFailed)
        {
            return paymentRequest.MarkAsyncPaymentFailed(snapshot.FailureReason, utcNow);
        }

        // The same step as the provider's webhook, so a monthly payment whose webhooks were missed still gets its
        // subscription, and a Direct Debit that has since been collected is marked paid.
        Result<Subscription?> completed = await checkoutCompletion.CompleteAsync(
            paymentRequest,
            new CompletedCheckout(
                snapshot.SessionId,
                snapshot.IsPaid,
                snapshot.PaymentIntentId,
                snapshot.CustomerId,
                snapshot.SubscriptionId),
            utcNow,
            cancellationToken);

        if (completed.IsSuccess && completed.Value is { } subscription && snapshot.SubscriptionStatus is { } status)
        {
            subscription.UpdateStatus(status, snapshot.SubscriptionPeriodEndUtc, utcNow, utcNow);
        }

        return completed;
    }
}
