using System.Globalization;

namespace SilverbridgeWeb.WebUI.Services.ScoreConnect;

internal enum LiveScoreState
{
    Unavailable,
    Offline,
    NotStarted,
    Live,
    FullTime
}

internal sealed record LiveScoreTeam(string Name, int Goals, int Points)
{
    public string Score => $"{Goals}-{Points.ToString("00", CultureInfo.InvariantCulture)}";

    public int Total => Goals * 3 + Points;
}

internal sealed record LiveScoreSnapshot(
    LiveScoreState State,
    bool BoardOnline,
    LiveScoreTeam? Home,
    LiveScoreTeam? Away,
    int ClockSeconds,
    bool ClockRunning,
    DateTimeOffset RetrievedAtUtc,
    string? Period = null)
{
    public bool IsLive => State == LiveScoreState.Live;

    public bool ShowScoreboard => State is LiveScoreState.NotStarted or LiveScoreState.Live or LiveScoreState.FullTime;

    public static LiveScoreSnapshot Unavailable(DateTimeOffset retrievedAtUtc) =>
        new(LiveScoreState.Unavailable, false, null, null, 0, false, retrievedAtUtc);

    public int GetClockSeconds(DateTimeOffset nowUtc)
    {
        double seconds = ClockSeconds;
        if (ClockRunning && nowUtc > RetrievedAtUtc)
        {
            seconds += (nowUtc - RetrievedAtUtc).TotalSeconds;
        }

        return (int)seconds;
    }

    public int GetElapsedMinutes(DateTimeOffset nowUtc) => GetClockSeconds(nowUtc) / 60;
}
