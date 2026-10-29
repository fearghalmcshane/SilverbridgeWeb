using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetMyPayments;

internal sealed class GetMyPaymentsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMyPaymentsQuery, MyPaymentsResponse>
{
    public async Task<Result<MyPaymentsResponse>> Handle(GetMyPaymentsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // Requests nobody started paying for are left out; they are not part of the payer's history.
        const string sql =
            $"""
             SELECT
                 id AS {nameof(PaymentSummaryResponse.Id)},
                 description AS {nameof(PaymentSummaryResponse.Description)},
                 source AS {nameof(PaymentSummaryResponse.Source)},
                 kind AS {nameof(PaymentSummaryResponse.Kind)},
                 currency AS {nameof(PaymentSummaryResponse.Currency)},
                 amount_minor AS {nameof(PaymentSummaryResponse.AmountMinor)},
                 amount_refunded_minor AS {nameof(PaymentSummaryResponse.AmountRefundedMinor)},
                 status AS {nameof(PaymentSummaryResponse.Status)},
                 created_at_utc AS {nameof(PaymentSummaryResponse.CreatedAtUtc)},
                 completed_at_utc AS {nameof(PaymentSummaryResponse.CompletedAtUtc)}
             FROM payments.payment_requests
             WHERE payer_user_id = @UserId AND status <> @Cancelled
             ORDER BY created_at_utc DESC
             LIMIT 100;

             SELECT
                 id AS {nameof(SubscriptionSummaryResponse.Id)},
                 description AS {nameof(SubscriptionSummaryResponse.Description)},
                 currency AS {nameof(SubscriptionSummaryResponse.Currency)},
                 amount_minor AS {nameof(SubscriptionSummaryResponse.AmountMinor)},
                 status AS {nameof(SubscriptionSummaryResponse.Status)},
                 current_period_end_utc AS {nameof(SubscriptionSummaryResponse.CurrentPeriodEndUtc)},
                 created_at_utc AS {nameof(SubscriptionSummaryResponse.CreatedAtUtc)}
             FROM payments.subscriptions
             WHERE payer_user_id = @UserId
             ORDER BY created_at_utc DESC;
             """;

        await using SqlMapper.GridReader grid = await connection.QueryMultipleAsync(
            sql,
            new { request.UserId, Cancelled = (int)PaymentRequestStatus.Cancelled });

        List<PaymentSummaryResponse> payments = (await grid.ReadAsync<PaymentSummaryResponse>()).AsList();
        List<SubscriptionSummaryResponse> subscriptions = (await grid.ReadAsync<SubscriptionSummaryResponse>()).AsList();

        return new MyPaymentsResponse(payments, subscriptions);
    }
}
