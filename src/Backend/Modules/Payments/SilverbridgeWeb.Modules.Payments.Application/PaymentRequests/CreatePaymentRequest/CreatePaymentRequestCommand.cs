using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CreatePaymentRequest;

public sealed record CreatePaymentRequestCommand(
    Guid PaymentRequestId,
    string Source,
    string SourceReference,
    Guid PayerUserId,
    string Description,
    string Currency,
    PaymentRequestKind Kind,
    IReadOnlyList<PaymentRequestLineDetails> Lines) : ICommand;
