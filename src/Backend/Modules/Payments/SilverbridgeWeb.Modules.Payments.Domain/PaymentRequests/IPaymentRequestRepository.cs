namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public interface IPaymentRequestRepository
{
    Task<PaymentRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaymentRequest?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentRequest>> GetUnsettledAsync(
        DateTime checkoutExpiredBeforeUtc,
        DateTime processingUpdatedBeforeUtc,
        int take,
        CancellationToken cancellationToken = default);

    void Insert(PaymentRequest paymentRequest);
}
