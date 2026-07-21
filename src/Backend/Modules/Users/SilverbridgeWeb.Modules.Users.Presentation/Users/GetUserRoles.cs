using System.Security.Claims;
using SilverbridgeWeb.Common.Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Infrastructure.Authentication;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Users.Application.Users.GetUserRoles;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class GetUserRoles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/roles", async (ClaimsPrincipal claims, ISender sender) =>
        {
            Result<UserRolesResponse> result = await sender.Send(new GetUserRolesQuery(claims.GetUserId()));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetUser)
        .WithTags(Tags.Users);
    }
}
