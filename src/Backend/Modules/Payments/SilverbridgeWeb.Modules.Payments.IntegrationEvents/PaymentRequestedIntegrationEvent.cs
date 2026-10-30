using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

/// <summary>
/// Published by a requesting module to ask Payments to collect money from a user.
/// The requesting module generates <see cref="PaymentRequestId"/> so it can send the user to
/// <c>/payments/{PaymentRequestId}</c> straight away.
/// </summary>
public sealed class PaymentRequestedIntegrationEvent : IntegrationEvent
{
    public PaymentRequestedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid paymentRequestId,
        string source,
        string sourceReference,
        Guid payerUserId,
        string description,
        string currency,
        PaymentKind kind,
        List<PaymentLineModel> lines)
        : base(id, occurredOnUtc)
    {
        PaymentRequestId = paymentRequestId;
        Source = source;
        SourceReference = sourceReference;
        PayerUserId = payerUserId;
        Description = description;
        Currency = currency;
        Kind = kind;
        Lines = lines;
    }

    public Guid PaymentRequestId { get; init; }

    public string Source { get; init; }

    public string SourceReference { get; init; }

    public Guid PayerUserId { get; init; }

    public string Description { get; init; }

    public string Currency { get; init; }

    public PaymentKind Kind { get; init; }

    public List<PaymentLineModel> Lines { get; init; }
}
