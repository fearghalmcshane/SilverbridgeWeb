using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using Stripe;
using CheckoutSessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using CheckoutSessionGetOptions = Stripe.Checkout.SessionGetOptions;
using CheckoutSessionLineItemOptions = Stripe.Checkout.SessionLineItemOptions;
using CheckoutSession = SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments.CheckoutSession;
using PortalSessionCreateOptions = Stripe.BillingPortal.SessionCreateOptions;
using StripeCheckoutSession = Stripe.Checkout.Session;
using StripeRefund = Stripe.Refund;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;

/// <summary>
/// Talks to Stripe. Every create call carries an idempotency key derived from our own identifiers, so a retry
/// after a timeout never charges or refunds twice.
/// </summary>
internal sealed class StripePaymentProvider(
    StripeClient stripeClient,
    IOptions<PaymentsOptions> options,
    ILogger<StripePaymentProvider> logger) : IPaymentProvider
{
    private Uri PublicBaseUrl => options.Value.PublicBaseUrl!;

    public async Task<Result<string>> CreateCustomerAsync(
        ProviderCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Customer customer = await stripeClient.V1.Customers.CreateAsync(
                new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = request.Name,
                    Metadata = new Dictionary<string, string> { [StripeMetadata.UserId] = request.UserId.ToString() }
                },
                new RequestOptions { IdempotencyKey = $"customer-{request.UserId:N}" },
                cancellationToken);

            return customer.Id;
        }
        catch (StripeException exception)
        {
            return Failure<string>(exception, "create a customer");
        }
    }

    public async Task<Result<CheckoutSession>> CreateCheckoutSessionAsync(
        CheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        bool isSubscription = request.Kind == PaymentRequestKind.MonthlySubscription;
        var metadata = new Dictionary<string, string>
        {
            [StripeMetadata.PaymentRequestId] = request.PaymentRequestId.ToString()
        };

        var sessionOptions = new CheckoutSessionCreateOptions
        {
            Mode = isSubscription ? "subscription" : "payment",
            ClientReferenceId = request.PaymentRequestId.ToString(),
            Customer = request.ProviderCustomerId,

            // Without a customer, Stripe asks for an email and keeps it so the receipt has somewhere to go.
            CustomerCreation = !isSubscription && request.ProviderCustomerId is null ? "always" : null,
            LineItems = request.Lines
                .Select(line => new CheckoutSessionLineItemOptions
                {
                    Quantity = line.Quantity,
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency.ToLowerInvariant(),
                        UnitAmount = line.UnitAmountMinor,
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = line.Description
                        },
                        Recurring = isSubscription
                            ? new Stripe.Checkout.SessionLineItemPriceDataRecurringOptions { Interval = "month" }
                            : null
                    }
                })
                .ToList(),
            Metadata = metadata,
            PaymentIntentData = isSubscription
                ? null
                : new Stripe.Checkout.SessionPaymentIntentDataOptions
                {
                    Description = request.Description,
                    Metadata = metadata
                },
            SubscriptionData = isSubscription
                ? new Stripe.Checkout.SessionSubscriptionDataOptions
                {
                    Description = request.Description,
                    Metadata = metadata
                }
                : null,
            SuccessUrl = $"{PayPageUrl(request.PaymentRequestId)}/complete?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{PayPageUrl(request.PaymentRequestId)}?cancelled=true",
            ExpiresAt = request.ExpiresAtUtc
        };

        try
        {
            StripeCheckoutSession session = await stripeClient.V1.Checkout.Sessions.CreateAsync(
                sessionOptions,
                new RequestOptions
                {
                    IdempotencyKey = $"checkout-{request.PaymentRequestId:N}-{request.ExpiresAtUtc.Ticks}"
                },
                cancellationToken);

            return new CheckoutSession(session.Id, new Uri(session.Url), session.ExpiresAt);
        }
        catch (StripeException exception)
        {
            return Failure<CheckoutSession>(exception, "start checkout");
        }
    }

    public async Task<Result<CheckoutSessionSnapshot>> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // The payment behind the session shows whether a completed but unpaid checkout failed or is still being collected.
            StripeCheckoutSession session = await stripeClient.V1.Checkout.Sessions.GetAsync(
                sessionId,
                new CheckoutSessionGetOptions { Expand = ["payment_intent", "subscription"] },
                cancellationToken: cancellationToken);

            return ToSnapshot(session);
        }
        catch (StripeException exception)
        {
            return Failure<CheckoutSessionSnapshot>(exception, "read the checkout session");
        }
    }

    public async Task<Result> ExpireCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            await stripeClient.V1.Checkout.Sessions.ExpireAsync(sessionId, cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (StripeException exception)
        {
            // Stripe refuses to expire a session that is no longer open; that is fine if it already expired.
            Result<CheckoutSessionSnapshot> snapshot = await GetCheckoutSessionAsync(sessionId, cancellationToken);
            if (snapshot.IsSuccess && snapshot.Value.State == CheckoutSessionState.Expired)
            {
                return Result.Success();
            }

            return Failure(exception, "close the checkout session");
        }
    }

    public async Task<Result<ProviderRefund>> CreateRefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            StripeRefund refund = await stripeClient.V1.Refunds.CreateAsync(
                new RefundCreateOptions
                {
                    PaymentIntent = request.PaymentIntentId,
                    Amount = request.AmountMinor,
                    Metadata = new Dictionary<string, string>
                    {
                        [StripeMetadata.PaymentRequestId] = request.PaymentRequestId.ToString(),
                        [StripeMetadata.RefundId] = request.RefundId.ToString()
                    }
                },
                new RequestOptions { IdempotencyKey = $"refund-{request.RefundId:N}" },
                cancellationToken);

            return new ProviderRefund(refund.Id, StripeStatusMapper.ToRefundStatus(refund.Status), refund.FailureReason);
        }
        catch (StripeException exception)
        {
            return Failure<ProviderRefund>(exception, "refund the payment");
        }
    }

    public async Task<Result> CancelSubscriptionAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await stripeClient.V1.Subscriptions.CancelAsync(
                providerSubscriptionId,
                cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (StripeException exception)
        {
            return Failure(exception, "cancel the subscription");
        }
    }

    public async Task<Result<Uri>> CreateBillingPortalSessionAsync(
        string providerCustomerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Stripe.BillingPortal.Session session = await stripeClient.V1.BillingPortal.Sessions.CreateAsync(
                new PortalSessionCreateOptions
                {
                    Customer = providerCustomerId,
                    ReturnUrl = new Uri(PublicBaseUrl, "account/payments").ToString()
                },
                cancellationToken: cancellationToken);

            return new Uri(session.Url);
        }
        catch (StripeException exception)
        {
            return Failure<Uri>(exception, "open the billing portal");
        }
    }

    internal static CheckoutSessionSnapshot ToSnapshot(StripeCheckoutSession session)
    {
        CheckoutSessionState state = session.Status switch
        {
            "complete" => CheckoutSessionState.Complete,
            "expired" => CheckoutSessionState.Expired,
            _ => CheckoutSessionState.Open
        };
        bool isPaid = StripeStatusMapper.IsPaid(session.PaymentStatus);
        bool paymentFailed =
            state == CheckoutSessionState.Complete &&
            !isPaid &&
            StripeStatusMapper.IsFailedPayment(session.PaymentIntent?.Status, session.Subscription?.Status);

        return new CheckoutSessionSnapshot(
            session.Id,
            state,
            isPaid,
            session.PaymentIntentId,
            session.CustomerId,
            session.SubscriptionId,
            paymentFailed,
            paymentFailed ? session.PaymentIntent?.LastPaymentError?.Message : null,
            session.Subscription is { } subscription ? StripeStatusMapper.ToSubscriptionStatus(subscription.Status) : null,
            session.Subscription is not null ? StripeStatusMapper.CurrentPeriodEnd(session.Subscription) : null);
    }

    private string PayPageUrl(Guid paymentRequestId) =>
        new Uri(PublicBaseUrl, $"payments/{paymentRequestId}").ToString();

    private Result Failure(StripeException exception, string operation)
    {
        logger.LogWarning(
            exception,
            "Stripe could not {Operation}: {StripeErrorCode}",
            operation,
            exception.StripeError?.Code);

        return Result.Failure(ProviderErrors.RequestFailed(operation));
    }

    private Result<T> Failure<T>(StripeException exception, string operation)
    {
        Result failure = Failure(exception, operation);

        return Result.Failure<T>(failure.Error);
    }
}
