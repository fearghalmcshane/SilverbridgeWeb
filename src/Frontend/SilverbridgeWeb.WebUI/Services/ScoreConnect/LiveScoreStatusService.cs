using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;

namespace SilverbridgeWeb.WebUI.Services.ScoreConnect;

/// <summary>
/// Reads the club scoreboard's current state from FSL ScoreConnect. Results are cached briefly and shared
/// across all circuits so visitors don't each poll the provider.
/// </summary>
internal sealed partial class LiveScoreStatusService(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<ScoreConnectOptions> options,
    TimeProvider timeProvider,
    ILogger<LiveScoreStatusService> logger) : ILiveScoreStatusProvider
{
    private const int MaxTeamNameLength = 40;

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FetchLocks = new(StringComparer.Ordinal);

    private readonly ScoreConnectOptions _options = options.Value;

    public TimeSpan RefreshInterval => _options.RefreshInterval;

    public TimeSpan ScoreboardRefreshInterval => _options.ScoreboardRefreshInterval;

    private string CacheKey => $"score-connect:{_options.BoardId}";

    public async Task<LiveScoreSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out LiveScoreSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        SemaphoreSlim fetchLock = FetchLocks.GetOrAdd(CacheKey, static _ => new SemaphoreSlim(1, 1));
        await fetchLock.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(CacheKey, out cached) && cached is not null)
            {
                return cached;
            }

            LiveScoreSnapshot snapshot = await FetchAsync(cancellationToken);
            cache.Set(CacheKey, snapshot, _options.CacheDuration);
            return snapshot;
        }
        finally
        {
            fetchLock.Release();
        }
    }

    private async Task<LiveScoreSnapshot> FetchAsync(CancellationToken cancellationToken)
    {
        string board = Uri.EscapeDataString(_options.BoardId);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"rest/v1/live_scores?mac=eq.{board}&select=*");
        request.Headers.Add("apikey", _options.PublishableKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.PublishableKey);
        request.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using Stream body = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
            return Parse(document.RootElement, timeProvider.GetUtcNow());
        }
        catch (Exception exception) when (
            exception is HttpRequestException or JsonException or ExecutionRejectedException
            || exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            LogFetchFailed(logger, _options.BoardId, exception);
            return LiveScoreSnapshot.Unavailable(timeProvider.GetUtcNow());
        }
    }

    internal static LiveScoreSnapshot Parse(JsonElement root, DateTimeOffset retrievedAtUtc)
    {
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0
            || root[0].ValueKind != JsonValueKind.Object)
        {
            return LiveScoreSnapshot.Unavailable(retrievedAtUtc);
        }

        JsonElement row = root[0];
        string? status = ReadString(row, "status");
        bool online = !row.TryGetProperty("online", out JsonElement onlineElement)
            || onlineElement.ValueKind != JsonValueKind.False;

        LiveScoreState state = status switch
        {
            "live" => LiveScoreState.Live,
            _ when !online => LiveScoreState.Offline,
            "finished" => LiveScoreState.FullTime,
            _ => LiveScoreState.NotStarted
        };

        var home = new LiveScoreTeam(
            TeamName(ReadString(row, "home_name"), "Home"),
            ReadCount(row, "home_goals"),
            ReadCount(row, "home_points"));
        var away = new LiveScoreTeam(
            TeamName(ReadString(row, "away_name"), "Away"),
            ReadCount(row, "away_goals"),
            ReadCount(row, "away_points"));

        bool clockRunning = state == LiveScoreState.Live
            && row.TryGetProperty("clock_running", out JsonElement running)
            && running.ValueKind == JsonValueKind.True;

        return new LiveScoreSnapshot(
            state,
            online,
            home,
            away,
            ReadCount(row, "clock_seconds", 100_000),
            clockRunning,
            retrievedAtUtc,
            ReadPeriod(row));
    }

    private static string? ReadPeriod(JsonElement row)
    {
        string? period = ReadString(row, "period")?.Trim();
        return string.IsNullOrEmpty(period) || period.Length > 12 ? null : period;
    }

    private static string TeamName(string? value, string fallback)
    {
        string trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return fallback;
        }

        return trimmed.Length > MaxTeamNameLength ? trimmed[..MaxTeamNameLength] : trimmed;
    }

    private static string? ReadString(JsonElement row, string name) =>
        row.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int ReadCount(JsonElement row, string name, int max = 1000)
    {
        if (!row.TryGetProperty(name, out JsonElement value))
        {
            return 0;
        }

        int number = value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out int parsed) => parsed,
            JsonValueKind.String when int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) => parsed,
            _ => 0
        };

        return number >= 0 && number < max ? number : 0;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Unable to read ScoreConnect status for board {BoardId}")]
    private static partial void LogFetchFailed(ILogger logger, string boardId, Exception exception);
}
