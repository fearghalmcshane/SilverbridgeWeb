using SilverbridgeWeb.Common.Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Users.Application.Users.AssignRole;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class AssignUserRole : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId:guid}/roles", async (Guid userId, Request request, ISender sender) =>
        {
            Result result = await sender.Send(new AssignRoleCommand(userId, request.RoleName));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyRoles)
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        public string RoleName { get; init; } = string.Empty;
    }
}
