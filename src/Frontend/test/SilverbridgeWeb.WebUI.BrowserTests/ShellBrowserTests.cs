using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit;

namespace SilverbridgeWeb.WebUI.BrowserTests;

[Collection(HarnessCollection.Name)]
public sealed class ShellBrowserTests(HarnessFixture harness) : PageTest
{
    [Fact]
    public async Task AppliesSystemDarkThemeAndRendersShell()
    {
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });

        await Page.GotoAsync(harness.Address);

        (await Page.Locator("html").GetAttributeAsync("data-theme-preference")).Should().Be("system");
        (await Page.Locator("html").EvaluateAsync<string>("element => element.style.colorScheme"))
            .Should().Be("dark");
        await Expect(Page.GetByRole(AriaRole.Banner)).ToContainTextAsync("Silverbridge Harps");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Club dashboard" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Navigation, new() { Name = "Primary navigation" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PersistsManualThemeOverrideAfterReload()
    {
        await Page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await Page.GotoAsync(harness.Address);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Light mode" }).ClickAsync();

        (await Page.Locator("html").GetAttributeAsync("data-theme-preference")).Should().Be("light");
        (await Page.Locator("html").EvaluateAsync<string>("element => element.style.colorScheme"))
            .Should().Be("light");

        await Page.ReloadAsync();

        (await Page.Locator("html").GetAttributeAsync("data-theme-preference")).Should().Be("light");
        (await Page.Locator("html").EvaluateAsync<string>("element => element.style.colorScheme"))
            .Should().Be("light");
    }

    [Fact]
    public async Task KeyboardSkipLinkMovesFocusToMainContent()
    {
        await Page.GotoAsync(harness.Address);

        await Page.Keyboard.PressAsync("Tab");
        (await Page.EvaluateAsync<string>("document.activeElement.className")).Should().Be("skip-link");

        await Page.Keyboard.PressAsync("Enter");

        (await Page.EvaluateAsync<string>("document.activeElement.id")).Should().Be("main-content");
    }

    [Fact]
    public async Task MobileViewportHasNoHorizontalOverflow()
    {
        await Page.SetViewportSizeAsync(375, 812);
        await Page.GotoAsync(harness.Address);

        bool hasNoOverflow = await Page.EvaluateAsync<bool>(
            "document.documentElement.scrollWidth <= document.documentElement.clientWidth");

        hasNoOverflow.Should().BeTrue();
    }
}
