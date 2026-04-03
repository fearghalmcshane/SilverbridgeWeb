namespace SilverbridgeWeb.Modules.Foireann.Infrastructure;

internal sealed class FoireannOptions
{
    public string BaseUrl { get; set; } = "https://api.foireann.ie/open-data/";

    public string PrimaryApiKey { get; set; } = string.Empty;

    public string SecondaryApiKey { get; set; } = string.Empty;

    public int CacheTtlMinutes { get; set; } = 15;
}
