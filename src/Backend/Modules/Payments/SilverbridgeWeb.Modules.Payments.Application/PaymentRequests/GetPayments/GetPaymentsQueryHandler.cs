using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPayments;

internal sealed class GetPaymentsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPaymentsQuery, PagedPaymentsResponse>
{
    public async Task<Result<PagedPaymentsResponse>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string filter =
            """
            WHERE (@Status IS NULL OR p.status = @Status)
              AND (@Search IS NULL
                   OR p.description ILIKE @Search
                   OR p.source_reference ILIKE @Search
                   OR pa.email ILIKE @Search
                   OR (pa.first_name || ' ' || pa.last_name) ILIKE @Search)
            """;

        const string sql =
            $"""
             SELECT COUNT(*)
             FROM payments.payment_requests p
             LEFT JOIN payments.payers pa ON pa.id = p.payer_user_id
             {filter};

             SELECT
                 p.id AS {nameof(AdminPaymentSummaryResponse.Id)},
                 p.payer_user_id AS {nameof(AdminPaymentSummaryResponse.PayerUserId)},
                 NULLIF(TRIM(pa.first_name || ' ' || pa.last_name), '') AS {nameof(AdminPaymentSummaryResponse.PayerName)},
                 pa.email AS {nameof(AdminPaymentSummaryResponse.PayerEmail)},
                 p.description AS {nameof(AdminPaymentSummaryResponse.Description)},
                 p.source AS {nameof(AdminPaymentSummaryResponse.Source)},
                 p.source_reference AS {nameof(AdminPaymentSummaryResponse.SourceReference)},
                 p.kind AS {nameof(AdminPaymentSummaryResponse.Kind)},
                 p.currency AS {nameof(AdminPaymentSummaryResponse.Currency)},
                 p.amount_minor AS {nameof(AdminPaymentSummaryResponse.AmountMinor)},
                 p.amount_refunded_minor AS {nameof(AdminPaymentSummaryResponse.AmountRefundedMinor)},
                 p.status AS {nameof(AdminPaymentSummaryResponse.Status)},
                 p.created_at_utc AS {nameof(AdminPaymentSummaryResponse.CreatedAtUtc)},
                 p.completed_at_utc AS {nameof(AdminPaymentSummaryResponse.CompletedAtUtc)}
             FROM payments.payment_requests p
             LEFT JOIN payments.payers pa ON pa.id = p.payer_user_id
             {filter}
             ORDER BY p.created_at_utc DESC
             LIMIT @PageSize OFFSET @Offset;
             """;

        string? search = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : $"%{EscapeLikePattern(request.Search.Trim())}%";

        await using SqlMapper.GridReader grid = await connection.QueryMultipleAsync(
            sql,
            new
            {
                Status = (int?)request.Status,
                Search = search,
                request.PageSize,
                Offset = (request.Page - 1) * request.PageSize
            });

        int totalCount = await grid.ReadSingleAsync<int>();
        List<AdminPaymentSummaryResponse> items = (await grid.ReadAsync<AdminPaymentSummaryResponse>()).AsList();

        return new PagedPaymentsResponse(items, totalCount, request.Page, request.PageSize);
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
