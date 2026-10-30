using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;

/// <param name="RefundId">
/// Chosen by the caller and reused when the same refund is retried, so a retry cannot refund twice.
/// </param>
/// <param name="AmountMinor">The amount to refund, or <see langword="null"/> to refund everything remaining.</param>
/// <param name="RequestedByUserId">The administrator asking, or <see langword="null"/> when another module asked.</param>
public sealed record RefundPaymentCommand(
    Guid PaymentRequestId,
    Guid RefundId,
    long? AmountMinor,
    string Reason,
    Guid? RequestedByUserId) : ICommand<Guid>;
