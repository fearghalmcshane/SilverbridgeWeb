using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed class RefundRequestedIntegrationEvent : IntegrationEvent
{
    public RefundRequestedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentRequestId,
        long? amountMinor,
        string reason)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
        AmountMinor = amountMinor;
        Reason = reason;
    }

    public Guid PaymentRequestId { get; init; }

    /// <summary>The amount to refund in minor units, or <see langword="null"/> to refund everything remaining.</summary>
    public long? AmountMinor { get; init; }

    public string Reason { get; init; }
}
