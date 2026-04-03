using System.Net.Http.Json;
using SilverbridgeWeb.Modules.Foireann.Application.Abstractions;
using SilverbridgeWeb.Modules.Foireann.Application.DTOs;

namespace SilverbridgeWeb.Modules.Foireann.Infrastructure;

internal sealed class FoireannClient(HttpClient httpClient)
{
    public async Task<FoireannPagedResponse<FoireannCompetitionResponse>> GetCompetitionsAsync(
        GetCompetitionsQuery query, CancellationToken cancellationToken = default)
    {
        string url = BuildCompetitionsUrl(query);
        return await httpClient.GetFromJsonAsync<FoireannPagedResponse<FoireannCompetitionResponse>>(
            url, cancellationToken) ?? EmptyPage<FoireannCompetitionResponse>();
    }

    public async Task<FoireannPagedResponse<FoireannFixtureResponse>> GetFixturesAsync(
        GetFixturesQuery query, CancellationToken cancellationToken = default)
    {
        string url = BuildFixturesUrl(query);
        return await httpClient.GetFromJsonAsync<FoireannPagedResponse<FoireannFixtureResponse>>(
            url, cancellationToken) ?? EmptyPage<FoireannFixtureResponse>();
    }

    private static string BuildCompetitionsUrl(GetCompetitionsQuery query)
    {
        var parameters = new List<string>
        {
            $"page={query.Page}",
            $"size={query.Size}"
        };

        if (!string.IsNullOrEmpty(query.Activity))
        {
            parameters.Add($"activity={Uri.EscapeDataString(query.Activity)}");
        }

        if (!string.IsNullOrEmpty(query.Grade))
        {
            parameters.Add($"grade={Uri.EscapeDataString(query.Grade)}");
        }

        if (!string.IsNullOrEmpty(query.Season))
        {
            parameters.Add($"season={Uri.EscapeDataString(query.Season)}");
        }

        if (!string.IsNullOrEmpty(query.Format))
        {
            parameters.Add($"format={Uri.EscapeDataString(query.Format)}");
        }

        if (!string.IsNullOrEmpty(query.AdditionalType))
        {
            parameters.Add($"additionalType={Uri.EscapeDataString(query.AdditionalType)}");
        }

        if (!string.IsNullOrEmpty(query.OwnerId))
        {
            parameters.Add($"owner.id={Uri.EscapeDataString(query.OwnerId)}");
        }

        if (!string.IsNullOrEmpty(query.Search))
        {
            parameters.Add($"search={Uri.EscapeDataString(query.Search)}");
        }

        if (!string.IsNullOrEmpty(query.Sort))
        {
            // Do not URI-encode sort: Spring REST APIs expect literal commas (e.g. startDate,asc)
            parameters.Add($"sort={query.Sort}");
        }

        return $"v1/competitions?{string.Join('&', parameters)}";
    }

    private static string BuildFixturesUrl(GetFixturesQuery query)
    {
        var parameters = new List<string>
        {
            $"page={query.Page}",
            $"size={query.Size}"
        };

        if (query.IsResult.HasValue)
        {
            parameters.Add($"isResult={query.IsResult.Value.ToString().ToLowerInvariant()}");
        }

        if (!string.IsNullOrEmpty(query.StartDateFrom))
        {
            parameters.Add($"startDateFrom={Uri.EscapeDataString(query.StartDateFrom)}");
        }

        if (!string.IsNullOrEmpty(query.StartDateTo))
        {
            parameters.Add($"startDateTo={Uri.EscapeDataString(query.StartDateTo)}");
        }

        if (!string.IsNullOrEmpty(query.CompetitionId))
        {
            parameters.Add($"competition.id={Uri.EscapeDataString(query.CompetitionId)}");
        }

        if (!string.IsNullOrEmpty(query.Activity))
        {
            parameters.Add($"competition.activity={Uri.EscapeDataString(query.Activity)}");
        }

        if (!string.IsNullOrEmpty(query.Grade))
        {
            parameters.Add($"competition.grade={Uri.EscapeDataString(query.Grade)}");
        }

        if (!string.IsNullOrEmpty(query.Search))
        {
            parameters.Add($"search={Uri.EscapeDataString(query.Search)}");
        }

        if (!string.IsNullOrEmpty(query.Sort))
        {
            // Do not URI-encode sort: Spring REST APIs expect literal commas (e.g. startDate,asc)
            parameters.Add($"sort={query.Sort}");
        }

        return $"v1/fixtures?{string.Join('&', parameters)}";
    }

    private static FoireannPagedResponse<T> EmptyPage<T>() =>
        new([], new(null, null, null, null, null), new(0, 0, 0, 0));
}
