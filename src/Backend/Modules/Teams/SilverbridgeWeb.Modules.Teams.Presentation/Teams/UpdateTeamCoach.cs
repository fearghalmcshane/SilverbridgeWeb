using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Teams.Application.Teams.UpdateTeamCoach;

namespace SilverbridgeWeb.Modules.Teams.Presentation.Teams;

internal sealed class UpdateTeamCoach : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("teams/{id}/coach", async (Guid id, Request request, ISender sender) =>
        {
            Result result = await sender.Send(new UpdateTeamCoachCommand(id, request.CoachName));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyTeams)
        .WithTags(Tags.Teams);
    }

    internal sealed class Request
    {
        public string? CoachName { get; init; }
    }
}
