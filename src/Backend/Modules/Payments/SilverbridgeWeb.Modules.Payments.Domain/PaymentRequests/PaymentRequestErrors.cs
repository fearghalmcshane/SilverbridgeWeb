using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public static class PaymentRequestErrors
{
    public static Error NotFound(Guid paymentRequestId) =>
        Error.NotFound("PaymentRequests.NotFound", $"The payment request with the identifier {paymentRequestId} was not found");

    public static Error NotFoundForPaymentIntent(string paymentIntentId) =>
        Error.NotFound("PaymentRequests.NotFound", $"No payment request matches the provider payment {paymentIntentId}");

    public static Error InvalidTransition(PaymentRequestStatus from, string action) =>
        Error.Conflict("PaymentRequests.InvalidTransition", $"A payment request that is {from} cannot be {action}");

    public static readonly Error UnsupportedCurrency = Error.Problem(
        "PaymentRequests.UnsupportedCurrency",
        $"Only {Currencies.Gbp} payments are supported");

    public static readonly Error NoLines = Error.Problem(
        "PaymentRequests.NoLines",
        "A payment request needs at least one line");

    public static readonly Error InvalidLine = Error.Problem(
        "PaymentRequests.InvalidLine",
        "Each line needs a description, a positive quantity and a positive unit amount");

    public static readonly Error NotPayable = Error.Conflict(
        "PaymentRequests.NotPayable",
        "This payment can no longer be paid");

    public static readonly Error NotRefundable = Error.Conflict(
        "PaymentRequests.NotRefundable",
        "Only completed one-off payments can be refunded");

    public static readonly Error InvalidRefundAmount = Error.Problem(
        "PaymentRequests.InvalidRefundAmount",
        "The refund amount must be positive and no more than the amount still refundable");

    public static Error RefundNotFound(Guid refundId) =>
        Error.NotFound("PaymentRequests.RefundNotFound", $"The refund with the identifier {refundId} was not found");
}
