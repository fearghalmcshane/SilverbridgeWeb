using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;

internal sealed class StartCheckoutCommandHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IPayerRepository payerRepository,
    ISubscriptionRepository subscriptionRepository,
    IPaymentProvider paymentProvider,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<StartCheckoutCommand, CheckoutRedirectResponse>
{
    internal static readonly TimeSpan CheckoutLifetime = TimeSpan.FromMinutes(30);

    public async Task<Result<CheckoutRedirectResponse>> Handle(
        StartCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        PaymentRequest? paymentRequest = await paymentRequestRepository.GetAsync(request.PaymentRequestId, cancellationToken);

        // Someone else's payment is reported as missing rather than forbidden, so identifiers cannot be probed.
        if (paymentRequest is null || paymentRequest.PayerUserId != request.UserId)
        {
            return Result.Failure<CheckoutRedirectResponse>(PaymentRequestErrors.NotFound(request.PaymentRequestId));
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        // Pressing Pay again, or coming back from the provider's page, reuses the session that is still open.
        if (paymentRequest.HasOpenCheckout(utcNow))
        {
            return new CheckoutRedirectResponse(new Uri(paymentRequest.CheckoutUrl!));
        }

        if (paymentRequest.Status is not (PaymentRequestStatus.Pending or PaymentRequestStatus.AwaitingPayment))
        {
            return Result.Failure<CheckoutRedirectResponse>(PaymentRequestErrors.NotPayable);
        }

        if (paymentRequest.ProviderCheckoutSessionId is not null)
        {
            // The previous session is about to lapse; close it so it cannot be paid alongside the new one.
            await paymentProvider.ExpireCheckoutSessionAsync(paymentRequest.ProviderCheckoutSessionId, cancellationToken);
        }

        Result<string?> customerResult = await EnsureProviderCustomerAsync(paymentRequest, cancellationToken);
        if (customerResult.IsFailure)
        {
            return Result.Failure<CheckoutRedirectResponse>(customerResult.Error);
        }

        Result<CheckoutSession> sessionResult = await paymentProvider.CreateCheckoutSessionAsync(
            new CheckoutSessionRequest(
                paymentRequest.Id,
                paymentRequest.Kind,
                paymentRequest.Description,
                paymentRequest.Currency,
                paymentRequest.Lines
                    .Select(l => new CheckoutLine(l.Description, l.Quantity, l.UnitAmountMinor))
                    .ToList(),
                customerResult.Value,
                utcNow.Add(CheckoutLifetime)),
            cancellationToken);

        if (sessionResult.IsFailure)
        {
            return Result.Failure<CheckoutRedirectResponse>(sessionResult.Error);
        }

        CheckoutSession session = sessionResult.Value;

        Result attachResult = paymentRequest.AttachCheckout(
            session.SessionId,
            session.Url,
            session.ExpiresAtUtc,
            customerResult.Value,
            utcNow);

        if (attachResult.IsFailure)
        {
            return Result.Failure<CheckoutRedirectResponse>(attachResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CheckoutRedirectResponse(session.Url);
    }

    /// <summary>
    /// Reuses or creates the payer's customer record at the provider so receipts go to their email address.
    /// Users that Payments has not heard about yet have no payer record to create one from: they reuse the customer
    /// from an earlier monthly payment, so all of theirs share one billing portal, or else the provider asks for an
    /// email itself.
    /// </summary>
    private async Task<Result<string?>> EnsureProviderCustomerAsync(
        PaymentRequest paymentRequest,
        CancellationToken cancellationToken)
    {
        Payer? payer = await payerRepository.GetAsync(paymentRequest.PayerUserId, cancellationToken);

        string? customerId = payer?.ProviderCustomerId ?? paymentRequest.ProviderCustomerId;
        if (customerId is null && payer is null)
        {
            customerId = await subscriptionRepository.GetLatestProviderCustomerIdAsync(
                paymentRequest.PayerUserId,
                cancellationToken);
        }

        if (customerId is not null || payer is null)
        {
            // Explicit, because converting a null value to a Result counts as a failure.
            return Result.Success(customerId);
        }

        Result<string> createResult = await paymentProvider.CreateCustomerAsync(
            new ProviderCustomerRequest(payer.Id, payer.Email, payer.FullName),
            cancellationToken);

        if (createResult.IsFailure)
        {
            return Result.Failure<string?>(createResult.Error);
        }

        payer.AssignProviderCustomer(createResult.Value);

        return createResult.Value;
    }
}
