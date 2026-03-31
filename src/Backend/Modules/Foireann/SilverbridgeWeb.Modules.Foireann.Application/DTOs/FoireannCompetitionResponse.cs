using System.Text.Json.Serialization;

namespace SilverbridgeWeb.Modules.Foireann.Application.DTOs;

public sealed record FoireannCompetitionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("legalName")] string? LegalName,
    [property: JsonPropertyName("sponsor")] string? Sponsor,
    [property: JsonPropertyName("additionalType")] string? AdditionalType,
    [property: JsonPropertyName("format")] string? Format,
    [property: JsonPropertyName("activity")] string? Activity,
    [property: JsonPropertyName("grade")] string? Grade,
    [property: JsonPropertyName("gender")] string? Gender,
    [property: JsonPropertyName("ageLevel")] string? AgeLevel,
    [property: JsonPropertyName("season")] string? Season,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("links")] FoireannLinkRef? Links);
