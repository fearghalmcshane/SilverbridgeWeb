using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPaymentRequest;

/// <param name="CanViewAll">Whether the requesting user may see payments that are not their own.</param>
public sealed record GetPaymentRequestQuery(Guid PaymentRequestId, Guid RequestingUserId, bool CanViewAll)
    : IQuery<PaymentRequestResponse>;
