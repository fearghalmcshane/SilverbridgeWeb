namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Subscription?> GetByProviderSubscriptionIdAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default);

    void Insert(Subscription subscription);
}
