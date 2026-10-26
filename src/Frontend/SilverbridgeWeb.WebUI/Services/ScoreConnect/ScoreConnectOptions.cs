namespace SilverbridgeWeb.WebUI.Services.ScoreConnect;

internal sealed class ScoreConnectOptions
{
    public const string SectionName = "ScoreConnect";

    public Uri ApiBaseUrl { get; set; } = new("https://uingnhvsgijsbfanrsry.supabase.co/");

    // FSL's publishable (public) key, also shipped to browsers in wwwroot/js/fslScoreboard.js.
    public string PublishableKey { get; set; } = "sb_publishable_d99Fz4rogMS79Q0AMve1oA_xwNCpKn2";

    public string BoardId { get; set; } = "SILVERBRIDGESB";

    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromSeconds(3);

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan ScoreboardRefreshInterval { get; set; } = TimeSpan.FromSeconds(3);

    // Off until FSL confirms the feed's period field tracks the match (it reads "H1" even with the board off).
    public bool ShowPeriod { get; set; }
}
