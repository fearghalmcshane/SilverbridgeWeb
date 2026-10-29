using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public sealed class PaymentRequestLine : Entity
{
    private PaymentRequestLine()
    {
    }

    public Guid Id { get; private set; }

    public Guid PaymentRequestId { get; private set; }

    public string Description { get; private set; }

    public int Quantity { get; private set; }

    public long UnitAmountMinor { get; private set; }

    public long AmountMinor => Quantity * UnitAmountMinor;

    internal static PaymentRequestLine Create(Guid paymentRequestId, string description, int quantity, long unitAmountMinor)
    {
        return new PaymentRequestLine
        {
            Id = Ulid.NewUlid().ToGuid(),
            PaymentRequestId = paymentRequestId,
            Description = description,
            Quantity = quantity,
            UnitAmountMinor = unitAmountMinor
        };
    }
}
