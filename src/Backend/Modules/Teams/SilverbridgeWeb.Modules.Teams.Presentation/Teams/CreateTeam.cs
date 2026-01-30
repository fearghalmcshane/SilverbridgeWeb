using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Teams.Application.Teams.CreateTeam;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Presentation.Teams;

internal sealed class CreateTeam : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("teams", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateTeamCommand(
                request.Name,
                request.AgeGroup,
                request.SportType,
                request.CoachName));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyTeams)
        .WithTags(Tags.Teams);
    }

    internal sealed class Request
    {
        public string Name { get; init; } = string.Empty;

        public AgeGroup AgeGroup { get; init; }

        public SportType SportType { get; init; }

        public string? CoachName { get; init; }
    }
}
