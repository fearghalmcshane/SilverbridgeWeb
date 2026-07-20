using SilverbridgeWeb.Common.Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Users.Application.Users.RemoveRole;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class RemoveUserRole : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{userId:guid}/roles/{roleName}", async (Guid userId, string roleName, ISender sender) =>
        {
            Result result = await sender.Send(new RemoveRoleCommand(userId, roleName));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyRoles)
        .WithTags(Tags.Users);
    }
}
