namespace SilverbridgeWeb.WebUI.Services.ScoreConnect;

/// <summary>
/// Periodically re-reads the scoreboard status for an interactive component until disposed.
/// </summary>
internal sealed class LiveScoreRefresher(
    ILiveScoreStatusProvider provider,
    Func<LiveScoreSnapshot, Task> onUpdated,
    TimeSpan? interval = null)
    : IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private bool _started;
    private bool _disposed;

    public void Start()
    {
        if (_started || _disposed)
        {
            return;
        }

        _started = true;
        _ = RunAsync(_cancellation.Token);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(interval ?? provider.RefreshInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                LiveScoreSnapshot snapshot = await provider.GetSnapshotAsync(cancellationToken);
                await onUpdated(snapshot);
            }
        }
        catch (Exception exception) when (exception is OperationCanceledException or ObjectDisposedException)
        {
            // The owning component was disposed.
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cancellation.Cancel();
        _cancellation.Dispose();
    }
}
