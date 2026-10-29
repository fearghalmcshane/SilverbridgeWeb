using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Webhooks.ApplyProviderEvent;

/// <summary>
/// Applies one provider event to the payment it concerns. A <see cref="ErrorType.NotFound"/> failure means the
/// event arrived before what it refers to and is worth retrying; a <see cref="ErrorType.Conflict"/> never will be.
/// </summary>
internal sealed class ApplyProviderEventCommandHandler(
    IPaymentRequestRepository paymentRequestRepository,
    ISubscriptionRepository subscriptionRepository,
    IPayerRepository payerRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ApplyProviderEventCommand>
{
    public async Task<Result> Handle(ApplyProviderEventCommand request, CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;

        Result result = request.Event switch
        {
            CheckoutCompletedProviderEvent e => await ApplyAsync(e, utcNow, cancellationToken),
            AsyncPaymentSucceededProviderEvent e => await WithPaymentRequestAsync(
                e.PaymentRequestId, p => p.MarkAsyncPaymentSucceeded(e.PaymentIntentId, utcNow), cancellationToken),
            AsyncPaymentFailedProviderEvent e => await WithPaymentRequestAsync(
                e.PaymentRequestId, p => p.MarkAsyncPaymentFailed("The payment could not be collected", utcNow), cancellationToken),
            CheckoutExpiredProviderEvent e => await WithPaymentRequestAsync(
                e.PaymentRequestId, p => p.MarkCheckoutExpired(e.SessionId, utcNow), cancellationToken),
            RefundUpdatedProviderEvent e => await ApplyAsync(e, utcNow, cancellationToken),
            SubscriptionUpdatedProviderEvent e => await ApplyAsync(e, utcNow, cancellationToken),
            InvoicePaymentProviderEvent e => await ApplyAsync(e, utcNow, cancellationToken),
            _ => Result.Success()
        };

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> WithPaymentRequestAsync(
        Guid paymentRequestId,
        Func<PaymentRequest, Result> apply,
        CancellationToken cancellationToken)
    {
        PaymentRequest? paymentRequest = await paymentRequestRepository.GetAsync(paymentRequestId, cancellationToken);

        return paymentRequest is null
            ? Result.Failure(PaymentRequestErrors.NotFound(paymentRequestId))
            : apply(paymentRequest);
    }

    private async Task<Result> ApplyAsync(
        CheckoutCompletedProviderEvent e,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        PaymentRequest? paymentRequest = await paymentRequestRepository.GetAsync(e.PaymentRequestId, cancellationToken);
        if (paymentRequest is null)
        {
            return Result.Failure(PaymentRequestErrors.NotFound(e.PaymentRequestId));
        }

        Result result = paymentRequest.MarkCheckoutCompleted(
            e.SessionId, e.IsPaid, e.PaymentIntentId, e.CustomerId, e.SubscriptionId, utcNow);
        if (result.IsFailure)
        {
            return result;
        }

        if (e.CustomerId is not null)
        {
            Payer? payer = await payerRepository.GetAsync(paymentRequest.PayerUserId, cancellationToken);
            if (payer is { ProviderCustomerId: null })
            {
                payer.AssignProviderCustomer(e.CustomerId);
            }
        }

        if (e.SubscriptionId is null)
        {
            return Result.Success();
        }

        Result<Subscription> subscription = await GetOrStartSubscriptionAsync(
            e.SubscriptionId, paymentRequest.Id, e.CustomerId, utcNow, cancellationToken);

        return subscription;
    }

    private async Task<Result> ApplyAsync(
        RefundUpdatedProviderEvent e,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        PaymentRequest? paymentRequest = null;
        if (e.PaymentRequestId is { } paymentRequestId)
        {
            paymentRequest = await paymentRequestRepository.GetAsync(paymentRequestId, cancellationToken);
        }

        if (paymentRequest is null && e.PaymentIntentId is not null)
        {
            paymentRequest = await paymentRequestRepository.GetByPaymentIntentIdAsync(e.PaymentIntentId, cancellationToken);
        }

        if (paymentRequest is null)
        {
            return Result.Failure(PaymentRequestErrors.NotFoundForPaymentIntent(e.PaymentIntentId ?? e.ProviderRefundId));
        }

        return paymentRequest.ApplyRefundUpdate(
            e.ProviderRefundId, e.RefundId, e.AmountMinor, e.Status, e.FailureReason, utcNow);
    }

    private async Task<Result> ApplyAsync(
        SubscriptionUpdatedProviderEvent e,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        Subscription? subscription =
            await subscriptionRepository.GetByProviderSubscriptionIdAsync(e.ProviderSubscriptionId, cancellationToken);

        if (subscription is null)
        {
            if (e.PaymentRequestId is null)
            {
                return Result.Failure(SubscriptionErrors.NotFoundForProvider(e.ProviderSubscriptionId));
            }

            Result<Subscription> started = await GetOrStartSubscriptionAsync(
                e.ProviderSubscriptionId, e.PaymentRequestId.Value, e.CustomerId, utcNow, cancellationToken);
            if (started.IsFailure)
            {
                return started;
            }

            subscription = started.Value;
        }

        subscription.UpdateStatus(e.Status, e.CurrentPeriodEndUtc, utcNow);

        return Result.Success();
    }

    private async Task<Result> ApplyAsync(
        InvoicePaymentProviderEvent e,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        Subscription? subscription =
            await subscriptionRepository.GetByProviderSubscriptionIdAsync(e.ProviderSubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure(SubscriptionErrors.NotFoundForProvider(e.ProviderSubscriptionId));
        }

        subscription.RecordPayment(
            e.ProviderInvoiceId,
            e.AmountMinor,
            e.Succeeded ? SubscriptionPaymentStatus.Succeeded : SubscriptionPaymentStatus.Failed,
            e.PeriodStartUtc,
            e.PeriodEndUtc,
            e.FailureReason,
            utcNow);

        return Result.Success();
    }

    private async Task<Result<Subscription>> GetOrStartSubscriptionAsync(
        string providerSubscriptionId,
        Guid paymentRequestId,
        string? customerId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        Subscription? existing = await subscriptionRepository.GetAsync(paymentRequestId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        PaymentRequest? paymentRequest = await paymentRequestRepository.GetAsync(paymentRequestId, cancellationToken);
        if (paymentRequest is null)
        {
            return Result.Failure<Subscription>(PaymentRequestErrors.NotFound(paymentRequestId));
        }

        Result<Subscription> started = Subscription.Start(paymentRequest, providerSubscriptionId, customerId, utcNow);
        if (started.IsSuccess)
        {
            subscriptionRepository.Insert(started.Value);
        }

        return started;
    }
}
