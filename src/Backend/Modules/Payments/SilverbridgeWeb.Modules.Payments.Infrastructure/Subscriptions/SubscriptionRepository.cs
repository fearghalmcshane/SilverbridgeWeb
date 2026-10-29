using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Subscriptions;

internal sealed class SubscriptionRepository(PaymentsDbContext context) : ISubscriptionRepository
{
    public async Task<Subscription?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Subscriptions
            .Include(s => s.Payments)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Subscription?> GetByProviderSubscriptionIdAsync(
        string providerSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await context.Subscriptions
            .Include(s => s.Payments)
            .SingleOrDefaultAsync(s => s.ProviderSubscriptionId == providerSubscriptionId, cancellationToken);
    }

    public void Insert(Subscription subscription)
    {
        context.Subscriptions.Add(subscription);
    }
}
