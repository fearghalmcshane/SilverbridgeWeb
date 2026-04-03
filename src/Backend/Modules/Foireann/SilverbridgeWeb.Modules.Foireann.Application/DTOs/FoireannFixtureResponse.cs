using System.Text.Json.Serialization;

namespace SilverbridgeWeb.Modules.Foireann.Application.DTOs;

public sealed record FoireannFixtureResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("homeTeam")] FoireannTeamResponse? HomeTeam,
    [property: JsonPropertyName("homeTeamFallbackName")] string? HomeTeamFallbackName,
    [property: JsonPropertyName("awayTeam")] FoireannTeamResponse? AwayTeam,
    [property: JsonPropertyName("awayTeamFallbackName")] string? AwayTeamFallbackName,
    [property: JsonPropertyName("startDate")] DateTimeOffset? StartDate,
    [property: JsonPropertyName("isResult")] bool IsResult,
    [property: JsonPropertyName("round")] string? Round,
    [property: JsonPropertyName("refereeName")] string? RefereeName,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("isPostponed")] bool? IsPostponed,
    [property: JsonPropertyName("isRescheduled")] bool? IsRescheduled,
    [property: JsonPropertyName("isReplay")] bool? IsReplay,
    [property: JsonPropertyName("isAbandoned")] bool? IsAbandoned,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("place")] FoireannPlaceResponse? Place,
    [property: JsonPropertyName("owner")] FoireannOwnerResponse? Owner,
    [property: JsonPropertyName("division")] FoireannDivisionResponse? Division,
    [property: JsonPropertyName("competition")] FoireannCompetitionResponse? Competition,
    [property: JsonPropertyName("links")] FoireannLinkRef? Links);

public sealed record FoireannTeamResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("designation")] string? Designation,
    [property: JsonPropertyName("goals")] int? Goals,
    [property: JsonPropertyName("points")] int? Points,
    [property: JsonPropertyName("penalties")] int? Penalties,
    [property: JsonPropertyName("isBye")] bool? IsBye,
    [property: JsonPropertyName("isConceded")] bool? IsConceded,
    [property: JsonPropertyName("logo")] string? Logo);

public sealed record FoireannPlaceResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("latitude")] double? Latitude,
    [property: JsonPropertyName("longitude")] double? Longitude);

public sealed record FoireannOwnerResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name);

public sealed record FoireannDivisionResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("abbreviatedName")] string? AbbreviatedName,
    [property: JsonPropertyName("format")] string? Format);
