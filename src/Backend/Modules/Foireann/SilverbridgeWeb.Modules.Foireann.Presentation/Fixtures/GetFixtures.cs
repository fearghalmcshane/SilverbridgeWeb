using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;
using SilverbridgeWeb.Modules.Foireann.Application.Fixtures;

namespace SilverbridgeWeb.Modules.Foireann.Presentation.Fixtures;

internal sealed class GetFixtures : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("foireann/fixtures", async (
            ISender sender,
            bool? isResult,
            string? startDateFrom,
            string? startDateTo,
            string? competitionId,
            string? activity,
            string? grade,
            string? search,
            int page = 0,
            int size = 20,
            string? sort = null) =>
        {
            Result<FoireannPagedResponse<FoireannFixtureResponse>> result = await sender.Send(
                new GetFixturesQuery(isResult, startDateFrom, startDateTo, competitionId, activity, grade, search, page, size, sort));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Foireann);
    }
}
