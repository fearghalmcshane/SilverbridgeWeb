﻿using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Ticketing.Domain.Customers;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Customers.GetCustomer;

internal sealed class GetCustomerByIdQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetCustomerQuery, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 id AS {nameof(CustomerResponse.Id)},
                 email AS {nameof(CustomerResponse.Email)},
                 first_name AS {nameof(CustomerResponse.FirstName)},
                 last_name AS {nameof(CustomerResponse.LastName)}
             FROM ticketing.customers
             WHERE id = @CustomerId
             """;

        CustomerResponse? customer = await connection.QuerySingleOrDefaultAsync<CustomerResponse>(sql, request);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound(request.CustomerId));
        }

        return customer;
    }
}
