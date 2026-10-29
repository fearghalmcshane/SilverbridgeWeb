using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetMyPayments;

namespace SilverbridgeWeb.Modules.Payments.Presentation.PaymentRequests;

internal sealed class GetMyPayments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("payments/me", async (IPaymentsUserContext userContext, ISender sender) =>
        {
            Result<MyPaymentsResponse> result = await sender.Send(new GetMyPaymentsQuery(userContext.UserId));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Payments);
    }
}
