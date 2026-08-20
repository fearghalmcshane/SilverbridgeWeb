using FluentAssertions;
using SilverbridgeWeb.WebUI.Services.Theme;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class ThemePreferenceResolverTests
{
    [Theory]
    [InlineData("Dark", false, "Dark", true)]
    [InlineData("Light", true, "Light", false)]
    [InlineData("System", true, "System", true)]
    [InlineData("system", false, "System", false)]
    [InlineData(null, true, "System", true)]
    [InlineData("unsupported", false, "System", false)]
    [InlineData("99", true, "System", true)]
    public void ResolvesPreferenceAndEffectiveTheme(
        string? storedPreference,
        bool systemIsDark,
        string expectedPreference,
        bool expectedIsDarkMode)
    {
        ThemePreferenceState result = ThemePreferenceResolver.Resolve(storedPreference, systemIsDark);

        result.Preference.ToString().Should().Be(expectedPreference);
        result.IsDarkMode.Should().Be(expectedIsDarkMode);
    }
}
