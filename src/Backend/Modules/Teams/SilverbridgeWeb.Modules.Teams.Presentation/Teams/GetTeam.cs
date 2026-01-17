using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeam;

namespace SilverbridgeWeb.Modules.Teams.Presentation.Teams;

internal sealed class GetTeam : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("teams/{id}", async (Guid id, ISender sender) =>
        {
            Result<TeamDetailResponse> result = await sender.Send(new GetTeamQuery(id));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetTeams)
        .WithTags(Tags.Teams);
    }
}
