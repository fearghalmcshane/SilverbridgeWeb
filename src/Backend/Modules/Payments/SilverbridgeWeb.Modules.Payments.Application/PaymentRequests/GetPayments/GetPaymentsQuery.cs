using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPayments;

/// <param name="Search">Matches the description, the source reference or the payer name or email.</param>
public sealed record GetPaymentsQuery(PaymentRequestStatus? Status, string? Search, int Page, int PageSize)
    : IQuery<PagedPaymentsResponse>;
