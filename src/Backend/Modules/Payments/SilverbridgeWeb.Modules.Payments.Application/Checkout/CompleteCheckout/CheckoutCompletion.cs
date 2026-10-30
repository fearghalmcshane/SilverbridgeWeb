using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Checkout.CompleteCheckout;

/// <summary>What the provider reports about a finished checkout, from its webhook or from asking it directly.</summary>
internal sealed record CompletedCheckout(
    string SessionId,
    bool IsPaid,
    string? PaymentIntentId,
    string? CustomerId,
    string? SubscriptionId);

/// <summary>
/// Records a finished checkout, whether the provider's webhook reported it or reconciliation found it: the payment
/// is updated, the payer is linked to the provider's customer, and a monthly payment gets its subscription.
/// </summary>
internal sealed class CheckoutCompletion(
    IPaymentRequestRepository paymentRequestRepository,
    ISubscriptionRepository subscriptionRepository,
    IPayerRepository payerRepository)
{
    public async Task<Result<Subscription?>> CompleteAsync(
        PaymentRequest paymentRequest,
        CompletedCheckout checkout,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        Result result = paymentRequest.MarkCheckoutCompleted(
            checkout.SessionId,
            checkout.IsPaid,
            checkout.PaymentIntentId,
            checkout.CustomerId,
            checkout.SubscriptionId,
            utcNow);
        if (result.IsFailure)
        {
            return Result.Failure<Subscription?>(result.Error);
        }

        if (checkout.CustomerId is not null)
        {
            Payer? payer = await payerRepository.GetAsync(paymentRequest.PayerUserId, cancellationToken);
            if (payer is { ProviderCustomerId: null })
            {
                payer.AssignProviderCustomer(checkout.CustomerId);
            }
        }

        if (checkout.SubscriptionId is null)
        {
            // Explicit, because converting a null value to a Result counts as a failure.
            return Result.Success<Subscription?>(null);
        }

        Result<Subscription> subscription = await GetOrStartSubscriptionAsync(
            paymentRequest, checkout.SubscriptionId, checkout.CustomerId, utcNow, cancellationToken);

        return subscription.IsSuccess
            ? subscription.Value
            : Result.Failure<Subscription?>(subscription.Error);
    }

    public async Task<Result<Subscription>> GetOrStartSubscriptionAsync(
        Guid paymentRequestId,
        string providerSubscriptionId,
        string? customerId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        PaymentRequest? paymentRequest = await paymentRequestRepository.GetAsync(paymentRequestId, cancellationToken);

        return paymentRequest is null
            ? Result.Failure<Subscription>(PaymentRequestErrors.NotFound(paymentRequestId))
            : await GetOrStartSubscriptionAsync(paymentRequest, providerSubscriptionId, customerId, utcNow, cancellationToken);
    }

    private async Task<Result<Subscription>> GetOrStartSubscriptionAsync(
        PaymentRequest paymentRequest,
        string providerSubscriptionId,
        string? customerId,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        Subscription? existing = await subscriptionRepository.GetAsync(paymentRequest.Id, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        Result<Subscription> started = Subscription.Start(paymentRequest, providerSubscriptionId, customerId, utcNow);
        if (started.IsSuccess)
        {
            subscriptionRepository.Insert(started.Value);
        }

        return started;
    }
}
