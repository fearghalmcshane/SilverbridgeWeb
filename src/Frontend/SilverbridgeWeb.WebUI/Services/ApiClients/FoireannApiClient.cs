using System.Globalization;
using System.Text.Json.Serialization;

namespace SilverbridgeWeb.WebUI.Services.ApiClients;

internal sealed class FoireannApiClient(HttpClient httpClient) : IApiClient
{
    public string BaseEndpoint => "/foireann";

    public async Task<FoireannPagedResponse<FoireannCompetitionDto>?> GetCompetitionsAsync(
        string? activity = null,
        string? grade = null,
        string? season = null,
        string? format = null,
        string? additionalType = null,
        string? ownerId = null,
        string? search = null,
        int page = 0,
        int size = 20,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        string url = BuildUrl($"{BaseEndpoint}/competitions", [
            ("activity", activity),
            ("grade", grade),
            ("season", season),
            ("format", format),
            ("additionalType", additionalType),
            ("ownerId", ownerId),
            ("search", search),
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("size", size.ToString(CultureInfo.InvariantCulture)),
            ("sort", sort)
        ]);

        return await httpClient.GetFromJsonAsync<FoireannPagedResponse<FoireannCompetitionDto>>(url, cancellationToken);
    }

    public async Task<FoireannPagedResponse<FoireannFixtureDto>?> GetFixturesAsync(
        bool? isResult = null,
        string? startDateFrom = null,
        string? startDateTo = null,
        string? competitionId = null,
        string? activity = null,
        string? grade = null,
        string? search = null,
        int page = 0,
        int size = 20,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        string url = BuildUrl($"{BaseEndpoint}/fixtures", [
            ("isResult", isResult?.ToString().ToLowerInvariant()),
            ("startDateFrom", startDateFrom),
            ("startDateTo", startDateTo),
            ("competitionId", competitionId),
            ("activity", activity),
            ("grade", grade),
            ("search", search),
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("size", size.ToString(CultureInfo.InvariantCulture)),
            ("sort", sort)
        ]);

        return await httpClient.GetFromJsonAsync<FoireannPagedResponse<FoireannFixtureDto>>(url, cancellationToken);
    }

    private static string BuildUrl(string path, (string Key, string? Value)[] parameters)
    {
        List<string> queryParams = [];

        foreach ((string key, string? value) in parameters)
        {
            if (!string.IsNullOrEmpty(value))
            {
                queryParams.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }
        }

        return queryParams.Count > 0 ? $"{path}?{string.Join('&', queryParams)}" : path;
    }
}

// Response DTOs matching the backend API shape

internal sealed record FoireannPagedResponse<T>(
    [property: JsonPropertyName("data")] IReadOnlyList<T> Data,
    [property: JsonPropertyName("links")] FoireannPaginationLinks Links,
    [property: JsonPropertyName("page")] FoireannPageInfo Page);

internal sealed record FoireannPageInfo(
    [property: JsonPropertyName("page")] long Page,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("totalPages")] long TotalPages,
    [property: JsonPropertyName("totalElements")] long TotalElements);

internal sealed record FoireannPaginationLinks(
    [property: JsonPropertyName("self")] FoireannLinkRef? Self,
    [property: JsonPropertyName("first")] FoireannLinkRef? First,
    [property: JsonPropertyName("next")] FoireannLinkRef? Next,
    [property: JsonPropertyName("prev")] FoireannLinkRef? Prev,
    [property: JsonPropertyName("last")] FoireannLinkRef? Last);

internal sealed record FoireannLinkRef(
    [property: JsonPropertyName("href")] string? Href);

internal sealed record FoireannCompetitionDto(
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
    [property: JsonPropertyName("status")] string? Status);

internal sealed record FoireannFixtureDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("homeTeam")] FoireannTeamDto? HomeTeam,
    [property: JsonPropertyName("homeTeamFallbackName")] string? HomeTeamFallbackName,
    [property: JsonPropertyName("awayTeam")] FoireannTeamDto? AwayTeam,
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
    [property: JsonPropertyName("place")] FoireannPlaceDto? Place,
    [property: JsonPropertyName("owner")] FoireannOwnerDto? Owner,
    [property: JsonPropertyName("division")] FoireannDivisionDto? Division,
    [property: JsonPropertyName("competition")] FoireannCompetitionDto? Competition);

internal sealed record FoireannTeamDto(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("designation")] string? Designation,
    [property: JsonPropertyName("goals")] int? Goals,
    [property: JsonPropertyName("points")] int? Points,
    [property: JsonPropertyName("penalties")] int? Penalties,
    [property: JsonPropertyName("isBye")] bool? IsBye,
    [property: JsonPropertyName("isConceded")] bool? IsConceded,
    [property: JsonPropertyName("logo")] string? Logo);

internal sealed record FoireannPlaceDto(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("latitude")] double? Latitude,
    [property: JsonPropertyName("longitude")] double? Longitude);

internal sealed record FoireannOwnerDto(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name);

internal sealed record FoireannDivisionDto(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("abbreviatedName")] string? AbbreviatedName,
    [property: JsonPropertyName("format")] string? Format);
