namespace SilverbridgeWeb.WebUI.Services.Documents;

/// <summary>
/// The club documents published on the site. Markdown lives in <c>Content/Documents/</c>.
/// </summary>
internal static class ClubDocumentCatalog
{
    public static ClubDocument Agm2026 { get; } = new(
        Slug: "agm-2026",
        Title: "AGM 2026 Booklet",
        Eyebrow: "Cruinniú Cinn Bhliana · Annual General Meeting",
        Description: "Agenda, minutes, reports, nominations, motions and accounts for the Annual General Meeting on 24 November 2026.",
        ResourceName: "ClubDocuments.agm-2026.md",
        PdfUrl: "/downloads/agm-2026-booklet.pdf");

    /// <summary>
    /// The booklet that <c>/agm</c> (and therefore the printed QR code) points at.
    /// Change this each year; QR codes already printed keep working.
    /// </summary>
    public static ClubDocument CurrentAgm => Agm2026;

    public static IReadOnlyList<ClubDocument> All { get; } = [Agm2026];

    public static ClubDocument? Find(string? slug) =>
        All.FirstOrDefault(document => string.Equals(document.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public static string PathFor(ClubDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return $"/documents/{document.Slug}";
    }
}
