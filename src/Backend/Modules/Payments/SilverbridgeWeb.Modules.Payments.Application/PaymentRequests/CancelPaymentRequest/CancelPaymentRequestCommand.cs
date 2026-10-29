using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CancelPaymentRequest;

public sealed record CancelPaymentRequestCommand(Guid PaymentRequestId) : ICommand;
