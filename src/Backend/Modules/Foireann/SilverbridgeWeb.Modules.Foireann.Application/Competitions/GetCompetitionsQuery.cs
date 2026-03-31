using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Application.Competitions;

public sealed record GetCompetitionsQuery(
    string? Activity,
    string? Grade,
    string? Season,
    string? Format,
    string? AdditionalType,
    string? OwnerId,
    string? Search,
    int Page,
    int Size,
    string? Sort) : IQuery<FoireannPagedResponse<FoireannCompetitionResponse>>;
