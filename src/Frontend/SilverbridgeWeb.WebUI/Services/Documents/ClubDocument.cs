namespace SilverbridgeWeb.WebUI.Services.Documents;

/// <summary>
/// A long-form club document (e.g. an AGM booklet) authored as Markdown and served as a web page.
/// </summary>
/// <param name="Slug">URL segment under <c>/documents/</c>.</param>
/// <param name="Title">Page title (the page's single <c>h1</c>).</param>
/// <param name="Eyebrow">Short label shown above the title.</param>
/// <param name="Description">Optional summary shown under the title.</param>
/// <param name="ResourceName">Embedded Markdown resource name (<c>ClubDocuments.{file}.md</c>).</param>
/// <param name="PdfUrl">Optional site path to a PDF copy, served from <c>wwwroot/downloads/</c>.</param>
internal sealed record ClubDocument(
    string Slug,
    string Title,
    string Eyebrow,
    string? Description,
    string ResourceName,
    string? PdfUrl);
