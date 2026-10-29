using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;

namespace SilverbridgeWeb.Modules.Payments.Presentation.Refunds;

internal sealed class RefundPayment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("payments/{id:guid}/refunds", async (
            Guid id,
            Request request,
            IPaymentsUserContext userContext,
            ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new RefundPaymentCommand(
                id,
                request.AmountMinor,
                request.Reason,
                userContext.UserId));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.RefundPayments)
        .WithTags(Tags.Payments);
    }

    internal sealed record Request(long? AmountMinor, string Reason);
}
