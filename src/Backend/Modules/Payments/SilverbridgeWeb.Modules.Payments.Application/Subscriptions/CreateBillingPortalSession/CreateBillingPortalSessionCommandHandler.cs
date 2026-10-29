using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;
using SilverbridgeWeb.Modules.Payments.Domain.Payers;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CreateBillingPortalSession;

/// <summary>Opens the provider's self-service page where a payer updates card or bank details and sees invoices.</summary>
internal sealed class CreateBillingPortalSessionCommandHandler(
    IPayerRepository payerRepository,
    IPaymentProvider paymentProvider)
    : ICommandHandler<CreateBillingPortalSessionCommand, CheckoutRedirectResponse>
{
    public async Task<Result<CheckoutRedirectResponse>> Handle(
        CreateBillingPortalSessionCommand request,
        CancellationToken cancellationToken)
    {
        Payer? payer = await payerRepository.GetAsync(request.UserId, cancellationToken);
        if (payer?.ProviderCustomerId is null)
        {
            return Result.Failure<CheckoutRedirectResponse>(PayerErrors.NoProviderCustomer);
        }

        Result<Uri> portalResult = await paymentProvider.CreateBillingPortalSessionAsync(
            payer.ProviderCustomerId,
            cancellationToken);

        return portalResult.IsSuccess
            ? new CheckoutRedirectResponse(portalResult.Value)
            : Result.Failure<CheckoutRedirectResponse>(portalResult.Error);
    }
}
