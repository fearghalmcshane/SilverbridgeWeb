namespace SilverbridgeWeb.WebUI.Services.Theme;

internal sealed class SystemThemeChangedEventArgs(bool isDarkMode) : EventArgs
{
    internal bool IsDarkMode { get; } = isDarkMode;
}
