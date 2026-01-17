using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Teams.Application.Teams.RemoveSquadMember;

namespace SilverbridgeWeb.Modules.Teams.Presentation.Teams;

internal sealed class RemoveSquadMember : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("teams/{teamId}/squad-members/{userId}", async (Guid teamId, Guid userId, ISender sender) =>
        {
            Result result = await sender.Send(new RemoveSquadMemberCommand(teamId, userId));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyTeams)
        .WithTags(Tags.Teams);
    }
}
