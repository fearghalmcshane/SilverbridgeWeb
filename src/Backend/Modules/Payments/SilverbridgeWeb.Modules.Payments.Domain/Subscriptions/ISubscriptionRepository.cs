namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Subscription?> GetByProviderSubscriptionIdAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>The provider customer behind the payer's most recent subscription, if any.</summary>
    Task<string?> GetLatestProviderCustomerIdAsync(Guid payerUserId, CancellationToken cancellationToken = default);

    void Insert(Subscription subscription);
}
