using System.Collections.Concurrent;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace SilverbridgeWeb.WebUI.Services.Documents;

internal sealed record DocumentSection(string Id, string Title, int Level);

internal sealed record RenderedDocument(string Html, IReadOnlyList<DocumentSection> Sections);

/// <summary>
/// Turns club-document Markdown into HTML plus a table of contents built from its headings.
/// </summary>
/// <remarks>
/// Raw HTML is allowed because documents are committed to the repository (pandoc emits HTML tables for
/// merged-cell Word tables). If documents ever become editable from the admin UI, sanitize the output first.
/// </remarks>
internal static class MarkdownDocumentRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private static readonly ConcurrentDictionary<string, RenderedDocument> Cache = new(StringComparer.Ordinal);

    public static RenderedDocument Render(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        MarkdownDocument document = Markdown.Parse(markdown, Pipeline);
        List<HeadingBlock> headings = [.. document.Descendants<HeadingBlock>()];

        // The page header owns the page's only h1, so push the document's headings down a level if it uses h1.
        if (headings.Exists(heading => heading.Level == 1))
        {
            foreach (HeadingBlock heading in headings)
            {
                heading.Level = Math.Min(heading.Level + 1, 6);
            }
        }

        List<DocumentSection> sections = [.. headings
            .Where(heading => heading.Level <= 3)
            .Select(heading => new DocumentSection(heading.GetAttributes().Id ?? string.Empty, GetText(heading), heading.Level))
            .Where(section => section.Id.Length > 0 && section.Title.Length > 0)];

        // Booklets carry dozens of photos; only fetch them as the reader scrolls near them.
        string html = document.ToHtml(Pipeline)
            .Replace("<img ", "<img loading=\"lazy\" decoding=\"async\" ", StringComparison.Ordinal);

        return new RenderedDocument(html, sections);
    }

    /// <summary>
    /// Renders an embedded club document, caching the result for the life of the process.
    /// </summary>
    public static RenderedDocument RenderResource(ClubDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return Cache.GetOrAdd(document.ResourceName, static name => Render(ReadResource(name)));
    }

    private static string ReadResource(string name)
    {
        using Stream stream = typeof(MarkdownDocumentRenderer).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Club document resource '{name}' was not found.");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static string GetText(HeadingBlock heading)
    {
        if (heading.Inline is null)
        {
            return string.Empty;
        }

        IEnumerable<string> parts = heading.Inline.Descendants<Inline>().Select(inline => inline switch
        {
            LiteralInline literal => literal.Content.ToString(),
            CodeInline code => code.Content,
            _ => string.Empty
        });

        return string.Concat(parts).Trim();
    }
}
