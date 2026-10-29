using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPayments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Presentation.PaymentRequests;

internal sealed class GetPayments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("payments", async (
            ISender sender,
            PaymentRequestStatus? status,
            string? search,
            int page = 1,
            int pageSize = 25) =>
        {
            Result<PagedPaymentsResponse> result = await sender.Send(new GetPaymentsQuery(status, search, page, pageSize));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ReadPayments)
        .WithTags(Tags.Payments);
    }
}
