using FluentAssertions;
using SilverbridgeWeb.WebUI.Services.Documents;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class ClubDocumentTests
{
    [Fact]
    public void BuildsContentsFromSecondAndThirdLevelHeadings()
    {
        RenderedDocument rendered = MarkdownDocumentRenderer.Render(
            "## Treasurer's Report\n\nText\n\n### Income\n\n#### Detail\n");

        rendered.Sections.Should().Equal(
            new DocumentSection("treasurers-report", "Treasurer's Report", 2),
            new DocumentSection("income", "Income", 3));
        rendered.Html.Should().Contain("<h2 id=\"treasurers-report\">");
    }

    [Fact]
    public void DemotesHeadingsWhenDocumentUsesH1SoThePageKeepsOneH1()
    {
        RenderedDocument rendered = MarkdownDocumentRenderer.Render("# Agenda\n\n## Item\n");

        rendered.Html.Should().NotContain("<h1").And.Contain("<h2 id=\"agenda\">").And.Contain("<h3 id=\"item\">");
        rendered.Sections.Select(section => section.Level).Should().Equal(2, 3);
    }

    [Fact]
    public void RendersPipeTables()
    {
        RenderedDocument rendered = MarkdownDocumentRenderer.Render("| Item | 2026 |\n| --- | ---: |\n| Income | £1 |\n");

        rendered.Html.Should().Contain("<table>").And.Contain("<td style=\"text-align: right;\">£1</td>");
    }

    [Fact]
    public void ImagesLoadLazily()
    {
        RenderedDocument rendered = MarkdownDocumentRenderer.Render("![Team](/a.jpg)\n\n<div class=\"doc-gallery\">\n<img src=\"/b.jpg\" alt=\"\">\n</div>\n");

        rendered.Html.Should().Contain("<img loading=\"lazy\" decoding=\"async\" src=\"/a.jpg\"")
            .And.Contain("<img loading=\"lazy\" decoding=\"async\" src=\"/b.jpg\"");
    }

    [Fact]
    public void EveryCatalogDocumentIsEmbeddedAndHasSections()
    {
        foreach (ClubDocument document in ClubDocumentCatalog.All)
        {
            RenderedDocument rendered = MarkdownDocumentRenderer.RenderResource(document);

            rendered.Sections.Should().NotBeEmpty(because: $"{document.Slug} should have navigable sections");
        }
    }

    [Fact]
    public void EveryFileACatalogDocumentLinksToExistsInWwwroot()
    {
        string wwwroot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "SilverbridgeWeb.WebUI", "wwwroot"));

        foreach (ClubDocument document in ClubDocumentCatalog.All)
        {
            string html = MarkdownDocumentRenderer.RenderResource(document).Html;
            IEnumerable<string> paths = System.Text.RegularExpressions.Regex
                .Matches(html, "(?:src|href)=\"(/(?:images|downloads)/[^\"]+)\"")
                .Select(match => match.Groups[1].Value)
                .Concat(document.PdfUrl is null ? [] : [document.PdfUrl]);

            foreach (string path in paths)
            {
                File.Exists(Path.Combine(wwwroot, path.TrimStart('/'))).Should().BeTrue(because: $"{document.Slug} links to {path}");
            }
        }
    }

    [Theory]
    [InlineData("http://silverbridgeharps.com/", "https://silverbridgeharps.com/agm")]
    [InlineData("http://example.org:8080/", "https://example.org/agm")]
    [InlineData("http://localhost:5123/", "http://localhost:5123/agm")]
    public void AgmLinkIsHttpsOffLocalhost(string siteAddress, string expected)
    {
        ClubDocumentEndpoints.PublicAgmUri(new Uri(siteAddress)).AbsoluteUri.Should().Be(expected);
    }

    [Fact]
    public void QrCodeIsScalableSvg()
    {
        string svg = ClubDocumentEndpoints.CreateQrCodeSvg(new Uri("https://example.org/agm"));

        svg.Should().Contain("<svg").And.Contain("viewBox");
    }
}
