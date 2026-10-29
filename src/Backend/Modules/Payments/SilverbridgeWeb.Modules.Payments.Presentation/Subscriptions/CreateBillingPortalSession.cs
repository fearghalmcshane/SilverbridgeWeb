using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;
using SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CreateBillingPortalSession;

namespace SilverbridgeWeb.Modules.Payments.Presentation.Subscriptions;

internal sealed class CreateBillingPortalSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("payments/billing-portal", async (IPaymentsUserContext userContext, ISender sender) =>
        {
            Result<CheckoutRedirectResponse> result =
                await sender.Send(new CreateBillingPortalSessionCommand(userContext.UserId));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Subscriptions);
    }
}
