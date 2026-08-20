using FluentAssertions;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class AppAssetReferencesTests
{
    [Fact]
    public void AppReferencesWebUiScopedCssBundle()
    {
        string appPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "SilverbridgeWeb.WebUI",
            "App.razor"));

        string appMarkup = File.ReadAllText(appPath);

        appMarkup.Should().Contain("SilverbridgeWeb.WebUI.styles.css");
        appMarkup.Should().NotContain("href=\"SilverbridgeWeb.styles.css\"");
    }

    [Fact]
    public void AccountAvatarUsesExplicitMenuToggle()
    {
        string componentPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "SilverbridgeWeb.WebUI",
            "Components",
            "LoginDisplay.razor"));

        string componentMarkup = File.ReadAllText(componentPath);

        componentMarkup.Should().Contain("@onclick=\"menuActivator.ToggleAsync\"");
        componentMarkup.Should().Contain("class=\"account-menu-button\"");
    }
}
