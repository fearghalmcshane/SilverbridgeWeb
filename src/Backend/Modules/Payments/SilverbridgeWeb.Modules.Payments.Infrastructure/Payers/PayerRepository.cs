using SilverbridgeWeb.Modules.Payments.Domain.Payers;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Payers;

internal sealed class PayerRepository(PaymentsDbContext context) : IPayerRepository
{
    public async Task<Payer?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Payers.FindAsync([id], cancellationToken);
    }

    public void Insert(Payer payer)
    {
        context.Payers.Add(payer);
    }
}
