using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;

/// <param name="AmountMinor">The amount to refund, or <see langword="null"/> to refund everything remaining.</param>
/// <param name="RequestedByUserId">The administrator asking, or <see langword="null"/> when another module asked.</param>
public sealed record RefundPaymentCommand(
    Guid PaymentRequestId,
    long? AmountMinor,
    string Reason,
    Guid? RequestedByUserId) : ICommand<Guid>;
