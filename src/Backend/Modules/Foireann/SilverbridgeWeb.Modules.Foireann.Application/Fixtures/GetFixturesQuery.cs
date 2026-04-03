using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Application.Fixtures;

public sealed record GetFixturesQuery(
    bool? IsResult,
    string? StartDateFrom,
    string? StartDateTo,
    string? CompetitionId,
    string? Activity,
    string? Grade,
    string? Search,
    int Page,
    int Size,
    string? Sort) : IQuery<FoireannPagedResponse<FoireannFixtureResponse>>;
