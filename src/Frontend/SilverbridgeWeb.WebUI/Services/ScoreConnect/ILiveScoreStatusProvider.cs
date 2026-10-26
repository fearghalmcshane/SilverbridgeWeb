namespace SilverbridgeWeb.WebUI.Services.ScoreConnect;

internal interface ILiveScoreStatusProvider
{
    TimeSpan RefreshInterval { get; }

    TimeSpan ScoreboardRefreshInterval { get; }

    Task<LiveScoreSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
