using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Infrastructure.Authentication;
using SilverbridgeWeb.Common.Presentation.Endpoints;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class GetCurrentUserPermissions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/permissions", (ClaimsPrincipal claims) =>
                Results.Ok(new Response(claims.GetPermissions())))
            .RequireAuthorization()
            .WithTags(Tags.Users);
    }

    internal sealed record Response(IReadOnlySet<string> Permissions);
}
