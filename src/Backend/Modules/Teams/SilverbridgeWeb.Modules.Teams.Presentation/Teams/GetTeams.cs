using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeams;

namespace SilverbridgeWeb.Modules.Teams.Presentation.Teams;

internal sealed class GetTeams : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("teams", async (ISender sender) =>
        {
            Result<IReadOnlyCollection<TeamResponse>> result = await sender.Send(new GetTeamsQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetTeams)
        .WithTags(Tags.Teams);
    }
}
