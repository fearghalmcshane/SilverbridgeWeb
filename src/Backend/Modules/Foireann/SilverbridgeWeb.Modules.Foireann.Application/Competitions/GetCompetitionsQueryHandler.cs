using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Foireann.Application.Abstractions;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Application.Competitions;

internal sealed class GetCompetitionsQueryHandler(IFoireannService foireannService)
    : IQueryHandler<GetCompetitionsQuery, FoireannPagedResponse<FoireannCompetitionResponse>>
{
    public async Task<Result<FoireannPagedResponse<FoireannCompetitionResponse>>> Handle(
        GetCompetitionsQuery request,
        CancellationToken cancellationToken)
    {
        FoireannPagedResponse<FoireannCompetitionResponse> response =
            await foireannService.GetCompetitionsAsync(
                new Abstractions.GetCompetitionsQuery(
                    request.Activity,
                    request.Grade,
                    request.Season,
                    request.Format,
                    request.AdditionalType,
                    request.OwnerId,
                    request.Search,
                    request.Page,
                    request.Size,
                    request.Sort),
                cancellationToken);

        return response;
    }
}
