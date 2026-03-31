using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Foireann.Application.Competitions;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Presentation.Competitions;

internal sealed class GetCompetitions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("foireann/competitions", async (
            ISender sender,
            string? activity,
            string? grade,
            string? season,
            string? format,
            string? additionalType,
            string? ownerId,
            string? search,
            int page = 0,
            int size = 20,
            string? sort = null) =>
        {
            Result<FoireannPagedResponse<FoireannCompetitionResponse>> result = await sender.Send(
                new GetCompetitionsQuery(activity, grade, season, format, additionalType, ownerId, search, page, size, sort));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Foireann);
    }
}
