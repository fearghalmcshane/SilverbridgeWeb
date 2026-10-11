namespace SilverbridgeWeb.Modules.Matches.Infrastructure.Outbox;

public sealed class MatchesOutboxOptions
{
    public int IntervalInSeconds { get; set; } = 10;

    public int BatchSize { get; set; } = 20;
}
