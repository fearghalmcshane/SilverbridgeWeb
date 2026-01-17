using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Teams.Application.Teams.AddSquadMember;

namespace SilverbridgeWeb.Modules.Teams.Presentation.Teams;

internal sealed class AddSquadMember : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("teams/{teamId}/squad-members", async (Guid teamId, Request request, ISender sender) =>
        {
            Result result = await sender.Send(new AddSquadMemberCommand(
                teamId,
                request.UserId,
                request.FirstName,
                request.LastName,
                request.JerseyNumber));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyTeams)
        .WithTags(Tags.Teams);
    }

    internal sealed class Request
    {
        public Guid UserId { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public int? JerseyNumber { get; init; }
    }
}
