using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using SilverbridgeWeb.WebUI.Components.Shared;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class StatePanelTests : IDisposable
{
    private readonly BunitContext _context = new();

    public StatePanelTests()
    {
        _context.Services.AddMudServices();
        _context.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RendersAccessibleStatusContentAndInvokesAction()
    {
        bool actionInvoked = false;

        IRenderedComponent<StatePanel> cut = _context.Render<StatePanel>(parameters => parameters
            .Add(component => component.Title, "No fixtures found")
            .Add(component => component.Description, "Try a different date range.")
            .Add(component => component.Actions, CreateAction("Reset filters", () => actionInvoked = true)));

        AngleSharp.Dom.IElement panel = cut.Find(".state-panel");
        AngleSharp.Dom.IElement title = cut.Find("h2");

        panel.GetAttribute("role").Should().Be("status");
        panel.GetAttribute("aria-live").Should().Be("polite");
        panel.GetAttribute("aria-labelledby").Should().Be(title.Id);
        title.TextContent.Should().Contain("No fixtures found");
        cut.Find(".state-panel__description").TextContent.Should().Contain("Try a different date range.");
        cut.Find(".state-panel__icon").GetAttribute("aria-hidden").Should().Be("true");

        cut.Find("button").Click();
        actionInvoked.Should().BeTrue();
    }

    [Fact]
    public void SupportsAlertSemanticsAndOmitsBlankOptionalContent()
    {
        IRenderedComponent<StatePanel> cut = _context.Render<StatePanel>(parameters => parameters
            .Add(component => component.Title, "Unable to load news")
            .Add(component => component.Description, " ")
            .Add(component => component.Role, "alert")
            .Add(component => component.AriaLive, "assertive"));

        AngleSharp.Dom.IElement panel = cut.Find(".state-panel");

        panel.GetAttribute("role").Should().Be("alert");
        panel.GetAttribute("aria-live").Should().Be("assertive");
        cut.FindAll(".state-panel__description").Should().BeEmpty();
        cut.FindAll(".state-panel__actions").Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static RenderFragment CreateAction(string label, Action action) => builder =>
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(action, action));
        builder.AddContent(3, label);
        builder.CloseElement();
    };
}
