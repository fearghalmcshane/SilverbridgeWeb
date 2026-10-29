using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPaymentRequest;

namespace SilverbridgeWeb.Modules.Payments.Presentation.PaymentRequests;

internal sealed class GetPaymentRequest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("payments/{id:guid}", async (Guid id, IPaymentsUserContext userContext, ISender sender) =>
        {
            Result<PaymentRequestResponse> result = await sender.Send(new GetPaymentRequestQuery(
                id,
                userContext.UserId,
                userContext.HasPermission(Permissions.ReadPayments)));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Payments);
    }
}
