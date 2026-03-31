using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Application.Abstractions;

public interface IFoireannService
{
    Task<FoireannPagedResponse<FoireannCompetitionResponse>> GetCompetitionsAsync(
        GetCompetitionsQuery query, CancellationToken cancellationToken = default);

    Task<FoireannPagedResponse<FoireannFixtureResponse>> GetFixturesAsync(
        GetFixturesQuery query, CancellationToken cancellationToken = default);
}

public sealed record GetCompetitionsQuery(
    string? Activity = null,
    string? Grade = null,
    string? Season = null,
    string? Format = null,
    string? AdditionalType = null,
    string? OwnerId = null,
    string? Search = null,
    int Page = 0,
    int Size = 20,
    string? Sort = null);

public sealed record GetFixturesQuery(
    bool? IsResult = null,
    string? StartDateFrom = null,
    string? StartDateTo = null,
    string? CompetitionId = null,
    string? Activity = null,
    string? Grade = null,
    string? Search = null,
    int Page = 0,
    int Size = 20,
    string? Sort = null);
