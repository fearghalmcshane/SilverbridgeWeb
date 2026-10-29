using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;

namespace SilverbridgeWeb.Modules.Payments.Presentation.PaymentRequests;

internal sealed class StartCheckout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("payments/{id:guid}/checkout", async (Guid id, IPaymentsUserContext userContext, ISender sender) =>
        {
            Result<CheckoutRedirectResponse> result = await sender.Send(new StartCheckoutCommand(id, userContext.UserId));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Payments);
    }
}
