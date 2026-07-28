using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Storage;
using SilverbridgeWeb.Modules.News.Application.Articles.AddArticleMedia;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class UploadArticleMedia : IEndpoint
{
    private const long MaxFileSizeInBytes = 25 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",
        [".mov"] = "video/quicktime"
    };

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("news/articles/{id}/media", async Task<IResult> (
            Guid id,
            IFormFile file,
            [FromForm] string? altText,
            [FromForm] int displayOrder,
            IFileStorageService fileStorageService,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (file.Length == 0)
            {
                return Results.BadRequest("A file is required.");
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                return Results.BadRequest("The file exceeds the 25MB size limit.");
            }

            string extension = Path.GetExtension(file.FileName);

            if (!AllowedContentTypes.TryGetValue(extension, out string? allowedContentType) ||
                !string.Equals(file.ContentType, allowedContentType, StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Only supported image and video files can be uploaded.");
            }

            string mediaType = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                ? "image"
                : "video";

            await using Stream content = file.OpenReadStream();

            string blobUrl = await fileStorageService.UploadAsync(
                content,
                file.FileName,
                file.ContentType,
                cancellationToken);

            Result<Guid> result = await sender.Send(
                new AddArticleMediaCommand
                {
                    ArticleId = id,
                    BlobUrl = blobUrl,
                    MediaType = mediaType,
                    AltText = altText,
                    DisplayOrder = displayOrder
                },
                cancellationToken);

            if (result.IsFailure)
            {
                await fileStorageService.DeleteAsync(blobUrl, cancellationToken);

                return ApiResults.Problem(result);
            }

            return Results.Ok(result.Value);
        })
        .RequireAuthorization(Permissions.UploadArticleMedia)
        .DisableAntiforgery()
        .WithTags(Tags.News);
    }
}
