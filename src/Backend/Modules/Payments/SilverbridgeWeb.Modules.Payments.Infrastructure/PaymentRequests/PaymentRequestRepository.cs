using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.PaymentRequests;

internal sealed class PaymentRequestRepository(PaymentsDbContext context) : IPaymentRequestRepository
{
    public async Task<PaymentRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.PaymentRequests
            .Include(p => p.Lines)
            .Include(p => p.Refunds)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PaymentRequest?> GetByPaymentIntentIdAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        return await context.PaymentRequests
            .Include(p => p.Lines)
            .Include(p => p.Refunds)
            .SingleOrDefaultAsync(p => p.ProviderPaymentIntentId == paymentIntentId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentRequest>> GetUnsettledAsync(
        DateTime checkoutExpiredBeforeUtc,
        DateTime processingUpdatedBeforeUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await context.PaymentRequests
            .Include(p => p.Lines)
            .Include(p => p.Refunds)
            .Where(p => p.ProviderCheckoutSessionId != null &&
                        (p.Status == PaymentRequestStatus.AwaitingPayment &&
                         p.CheckoutExpiresAtUtc < checkoutExpiredBeforeUtc &&
                         p.UpdatedAtUtc < checkoutExpiredBeforeUtc ||
                         p.Status == PaymentRequestStatus.Processing &&
                         p.UpdatedAtUtc < processingUpdatedBeforeUtc))
            .OrderBy(p => p.UpdatedAtUtc)
            .Take(take)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public void Insert(PaymentRequest paymentRequest)
    {
        context.PaymentRequests.Add(paymentRequest);
    }
}
