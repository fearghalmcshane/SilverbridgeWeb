using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Foireann.Application.Abstractions;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Application.Fixtures;

internal sealed class GetFixturesQueryHandler(IFoireannService foireannService)
    : IQueryHandler<GetFixturesQuery, FoireannPagedResponse<FoireannFixtureResponse>>
{
    public async Task<Result<FoireannPagedResponse<FoireannFixtureResponse>>> Handle(
        GetFixturesQuery request,
        CancellationToken cancellationToken)
    {
        FoireannPagedResponse<FoireannFixtureResponse> response =
            await foireannService.GetFixturesAsync(
                new Abstractions.GetFixturesQuery(
                    request.IsResult,
                    request.StartDateFrom,
                    request.StartDateTo,
                    request.CompetitionId,
                    request.Activity,
                    request.Grade,
                    request.Search,
                    request.Page,
                    request.Size,
                    request.Sort),
                cancellationToken);

        return response;
    }
}
