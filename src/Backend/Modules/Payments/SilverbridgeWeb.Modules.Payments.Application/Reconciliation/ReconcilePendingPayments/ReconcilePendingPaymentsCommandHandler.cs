using Microsoft.Extensions.Logging;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.Reconciliation.ReconcilePendingPayments;

/// <summary>
/// Catches payments whose webhook never arrived: any checkout that should have finished by now is looked up
/// at the provider and its real state applied.
/// </summary>
internal sealed class ReconcilePendingPaymentsCommandHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IPaymentProvider paymentProvider,
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

            Result result = Apply(paymentRequest, snapshot.Value, utcNow);
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

    private static Result Apply(PaymentRequest paymentRequest, CheckoutSessionSnapshot snapshot, DateTime utcNow)
    {
        return snapshot.State switch
        {
            CheckoutSessionState.Expired => paymentRequest.MarkCheckoutExpired(snapshot.SessionId, utcNow),
            CheckoutSessionState.Complete when snapshot.PaymentFailed =>
                paymentRequest.MarkAsyncPaymentFailed(snapshot.FailureReason, utcNow),
            CheckoutSessionState.Complete when paymentRequest.Status == PaymentRequestStatus.Processing && snapshot.IsPaid =>
                paymentRequest.MarkAsyncPaymentSucceeded(snapshot.PaymentIntentId, utcNow),
            CheckoutSessionState.Complete => paymentRequest.MarkCheckoutCompleted(
                snapshot.SessionId,
                snapshot.IsPaid,
                snapshot.PaymentIntentId,
                snapshot.CustomerId,
                snapshot.SubscriptionId,
                utcNow),
            _ => Result.Success()
        };
    }
}
