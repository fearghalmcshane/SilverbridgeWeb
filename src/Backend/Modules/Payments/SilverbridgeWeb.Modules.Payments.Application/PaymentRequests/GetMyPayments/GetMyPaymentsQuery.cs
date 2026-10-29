using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetMyPayments;

public sealed record GetMyPaymentsQuery(Guid UserId) : IQuery<MyPaymentsResponse>;
