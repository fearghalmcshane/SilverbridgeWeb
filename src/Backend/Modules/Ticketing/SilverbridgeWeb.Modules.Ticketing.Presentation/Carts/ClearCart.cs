using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Ticketing.Application.Carts.ClearCart;

namespace SilverbridgeWeb.Modules.Ticketing.Presentation.Carts;

internal sealed class ClearCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("carts", async (ICustomerContext customerContext, ISender sender) =>
        {
            Result result = await sender.Send(new ClearCartCommand(customerContext.CustomerId));

            return result.Match(() => Results.Ok(), ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.RemoveFromCart)
        .WithTags(Tags.Carts);
    }
}
