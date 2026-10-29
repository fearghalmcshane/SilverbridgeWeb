using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

/// <summary>
/// Outbound calls to a payment provider. Implementations build the payer's return addresses themselves
/// from configuration, so callers can never steer a redirect.
/// </summary>
public interface IPaymentProvider
{
    Task<Result<string>> CreateCustomerAsync(ProviderCustomerRequest request, CancellationToken cancellationToken = default);

    Task<Result<CheckoutSession>> CreateCheckoutSessionAsync(
        CheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CheckoutSessionSnapshot>> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<Result> ExpireCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<Result<ProviderRefund>> CreateRefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);

    Task<Result<Uri>> CreateBillingPortalSessionAsync(
        string providerCustomerId,
        CancellationToken cancellationToken = default);
}
