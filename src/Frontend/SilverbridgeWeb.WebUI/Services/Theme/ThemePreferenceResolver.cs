namespace SilverbridgeWeb.WebUI.Services.Theme;

internal static class ThemePreferenceResolver
{
    internal static ThemePreferenceState Resolve(string? preference, bool systemIsDark)
    {
        ThemePreference resolvedPreference = Enum.TryParse(
            preference,
            ignoreCase: true,
            out ThemePreference parsedPreference) &&
            Enum.IsDefined(parsedPreference)
                ? parsedPreference
                : ThemePreference.System;

        bool isDarkMode = resolvedPreference switch
        {
            ThemePreference.Dark => true,
            ThemePreference.Light => false,
            _ => systemIsDark
        };

        return new(resolvedPreference, isDarkMode);
    }
}
