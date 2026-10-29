using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPaymentRequest;

internal sealed class GetPaymentRequestQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPaymentRequestQuery, PaymentRequestResponse>
{
    public async Task<Result<PaymentRequestResponse>> Handle(
        GetPaymentRequestQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 id AS {nameof(PaymentRequestResponse.Id)},
                 payer_user_id AS {nameof(PaymentRequestResponse.PayerUserId)},
                 source AS {nameof(PaymentRequestResponse.Source)},
                 source_reference AS {nameof(PaymentRequestResponse.SourceReference)},
                 description AS {nameof(PaymentRequestResponse.Description)},
                 kind AS {nameof(PaymentRequestResponse.Kind)},
                 currency AS {nameof(PaymentRequestResponse.Currency)},
                 amount_minor AS {nameof(PaymentRequestResponse.AmountMinor)},
                 amount_refunded_minor AS {nameof(PaymentRequestResponse.AmountRefundedMinor)},
                 status AS {nameof(PaymentRequestResponse.Status)},
                 failure_reason AS {nameof(PaymentRequestResponse.FailureReason)},
                 created_at_utc AS {nameof(PaymentRequestResponse.CreatedAtUtc)},
                 completed_at_utc AS {nameof(PaymentRequestResponse.CompletedAtUtc)}
             FROM payments.payment_requests
             WHERE id = @PaymentRequestId;

             SELECT
                 description AS {nameof(PaymentRequestLineResponse.Description)},
                 quantity AS {nameof(PaymentRequestLineResponse.Quantity)},
                 unit_amount_minor AS {nameof(PaymentRequestLineResponse.UnitAmountMinor)}
             FROM payments.payment_request_lines
             WHERE payment_request_id = @PaymentRequestId
             ORDER BY id;

             SELECT
                 id AS {nameof(RefundResponse.Id)},
                 amount_minor AS {nameof(RefundResponse.AmountMinor)},
                 reason AS {nameof(RefundResponse.Reason)},
                 status AS {nameof(RefundResponse.Status)},
                 failure_reason AS {nameof(RefundResponse.FailureReason)},
                 created_at_utc AS {nameof(RefundResponse.CreatedAtUtc)},
                 completed_at_utc AS {nameof(RefundResponse.CompletedAtUtc)}
             FROM payments.refunds
             WHERE payment_request_id = @PaymentRequestId
             ORDER BY created_at_utc;
             """;

        await using SqlMapper.GridReader grid = await connection.QueryMultipleAsync(
            sql,
            new { request.PaymentRequestId });

        PaymentRequestResponse? paymentRequest = await grid.ReadSingleOrDefaultAsync<PaymentRequestResponse>();

        // Someone else's payment is reported as missing rather than forbidden, so identifiers cannot be probed.
        if (paymentRequest is null || !request.CanViewAll && paymentRequest.PayerUserId != request.RequestingUserId)
        {
            return Result.Failure<PaymentRequestResponse>(PaymentRequestErrors.NotFound(request.PaymentRequestId));
        }

        paymentRequest.Lines.AddRange(await grid.ReadAsync<PaymentRequestLineResponse>());
        paymentRequest.Refunds.AddRange(await grid.ReadAsync<RefundResponse>());

        return paymentRequest;
    }
}
