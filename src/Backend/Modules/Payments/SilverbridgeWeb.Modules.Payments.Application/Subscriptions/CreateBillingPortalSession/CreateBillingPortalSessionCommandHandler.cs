using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CreateBillingPortalSession;

/// <summary>Opens the provider's self-service page where a payer updates card or bank details and sees invoices.</summary>
internal sealed class CreateBillingPortalSessionCommandHandler(
    IPayerRepository payerRepository,
    ISubscriptionRepository subscriptionRepository,
    IPaymentProvider paymentProvider)
    : ICommandHandler<CreateBillingPortalSessionCommand, CheckoutRedirectResponse>
{
    public async Task<Result<CheckoutRedirectResponse>> Handle(
        CreateBillingPortalSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Users who signed up before Payments existed have no payer record; their subscription still knows the customer.
        Payer? payer = await payerRepository.GetAsync(request.UserId, cancellationToken);
        string? customerId =
            payer?.ProviderCustomerId ??
            await subscriptionRepository.GetLatestProviderCustomerIdAsync(request.UserId, cancellationToken);

        if (customerId is null)
        {
            return Result.Failure<CheckoutRedirectResponse>(PayerErrors.NoProviderCustomer);
        }

        Result<Uri> portalResult = await paymentProvider.CreateBillingPortalSessionAsync(customerId, cancellationToken);

        return portalResult.IsSuccess
            ? new CheckoutRedirectResponse(portalResult.Value)
            : Result.Failure<CheckoutRedirectResponse>(portalResult.Error);
    }
}
