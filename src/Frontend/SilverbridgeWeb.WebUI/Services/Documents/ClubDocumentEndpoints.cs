using Microsoft.AspNetCore.Http.HttpResults;
using QRCoder;

namespace SilverbridgeWeb.WebUI.Services.Documents;

/// <summary>
/// Short links and QR codes for club documents.
/// </summary>
internal static class ClubDocumentEndpoints
{
    public const string AgmPath = "/agm";
    public const string AgmQrCodePath = "/agm/qr.svg";
    public const string AgmPdfPath = "/agm/pdf";

    /// <summary>
    /// Keeps club documents out of search engines; they are shared by link and QR code only.
    /// </summary>
    public static void UseNoIndexForClubDocuments(this IApplicationBuilder app)
    {
        app.Use((context, next) =>
        {
            PathString path = context.Request.Path;
            if (path.StartsWithSegments("/documents", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/downloads", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments(AgmPath, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
            }

            return next(context);
        });
    }

    public static void MapClubDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        // Temporary redirect so next year's booklet can reuse the same printed QR code.
        app.MapGet(AgmPath, () => TypedResults.Redirect(ClubDocumentCatalog.PathFor(ClubDocumentCatalog.CurrentAgm)))
            .AllowAnonymous();

        // Shareable direct link to the current booklet's PDF.
        app.MapGet(AgmPdfPath, Results<RedirectHttpResult, NotFound> () =>
            ClubDocumentCatalog.CurrentAgm.PdfUrl is { } pdfUrl ? TypedResults.Redirect(pdfUrl) : TypedResults.NotFound())
            .AllowAnonymous();

        app.MapGet(AgmQrCodePath, (HttpContext context) =>
        {
            Uri target = PublicAgmUri(new Uri($"{context.Request.Scheme}://{context.Request.Host}/"));
            return TypedResults.Text(CreateQrCodeSvg(target), "image/svg+xml");
        })
        .AllowAnonymous();
    }

    /// <summary>
    /// The absolute <c>/agm</c> link for a site base URI. Forces https off localhost because TLS terminates
    /// at the Container Apps ingress, so the app itself may see plain http.
    /// </summary>
    public static Uri PublicAgmUri(Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(baseUri);

        UriBuilder builder = new(baseUri) { Path = AgmPath, Query = string.Empty, Fragment = string.Empty };
        if (!baseUri.IsLoopback)
        {
            builder.Scheme = Uri.UriSchemeHttps;
            builder.Port = -1;
        }

        return builder.Uri;
    }

    public static string CreateQrCodeSvg(Uri target)
    {
        ArgumentNullException.ThrowIfNull(target);

        using QRCodeGenerator generator = new();
        using QRCodeData data = generator.CreateQrCode(target.AbsoluteUri, QRCodeGenerator.ECCLevel.Q);
        using SvgQRCode qrCode = new(data);
        return qrCode.GetGraphic(10, "#000000", "#FFFFFF", true, SvgQRCode.SizingMode.ViewBoxAttribute);
    }
}
