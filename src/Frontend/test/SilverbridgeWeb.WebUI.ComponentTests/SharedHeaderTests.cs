using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using SilverbridgeWeb.WebUI.Components.Shared;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class SharedHeaderTests : IDisposable
{
    private readonly BunitContext _context = new();

    public SharedHeaderTests()
    {
        _context.Services.AddMudServices();
        _context.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void PageHeaderRendersSemanticContentAndInvokesAction()
    {
        bool actionInvoked = false;

        IRenderedComponent<PageHeader> cut = _context.Render<PageHeader>(parameters => parameters
            .Add(component => component.Title, "Club news")
            .Add(component => component.Eyebrow, "Latest")
            .Add(component => component.Description, "Updates from across the club.")
            .Add(component => component.Actions, CreateAction("Create article", () => actionInvoked = true)));

        AngleSharp.Dom.IElement header = cut.Find("header");
        AngleSharp.Dom.IElement title = cut.Find("h1");

        header.GetAttribute("aria-labelledby").Should().Be(title.Id);
        title.TextContent.Should().Be("Club news");
        cut.Find(".page-header__eyebrow").TextContent.Should().Be("Latest");
        cut.Find(".page-header__description").TextContent.Should().Be("Updates from across the club.");

        cut.Find("button").Click();
        actionInvoked.Should().BeTrue();
    }

    [Fact]
    public void SectionHeaderRendersSemanticContentAndInvokesAction()
    {
        bool actionInvoked = false;

        IRenderedComponent<SectionHeader> cut = _context.Render<SectionHeader>(parameters => parameters
            .Add(component => component.Title, "Upcoming fixtures")
            .Add(component => component.Description, "The next matches.")
            .Add(component => component.Actions, CreateAction("View all", () => actionInvoked = true)));

        AngleSharp.Dom.IElement header = cut.Find("header");
        AngleSharp.Dom.IElement title = cut.Find("h2");

        header.GetAttribute("aria-labelledby").Should().Be(title.Id);
        title.TextContent.Should().Be("Upcoming fixtures");
        cut.Find("p").TextContent.Should().Be("The next matches.");

        cut.Find("button").Click();
        actionInvoked.Should().BeTrue();
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
