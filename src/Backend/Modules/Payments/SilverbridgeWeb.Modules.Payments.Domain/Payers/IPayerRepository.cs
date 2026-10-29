namespace SilverbridgeWeb.Modules.Payments.Domain.Payers;

public interface IPayerRepository
{
    Task<Payer?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Payer payer);
}
