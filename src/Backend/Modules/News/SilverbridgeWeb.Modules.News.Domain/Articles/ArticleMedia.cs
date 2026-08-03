using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticleMedia : Entity
{
    private ArticleMedia()
    {
    }

    public Guid Id { get; private set; }

    public Guid ArticleId { get; private set; }

    public string BlobUrl { get; private set; }

    public string MediaType { get; private set; }

    public string? AltText { get; private set; }

    public int DisplayOrder { get; private set; }

    internal static ArticleMedia Create(
        Guid articleId,
        string blobUrl,
        string mediaType,
        string? altText,
        int displayOrder)
    {
        return new ArticleMedia
        {
            Id = Ulid.NewUlid().ToGuid(),
            ArticleId = articleId,
            BlobUrl = blobUrl,
            MediaType = mediaType,
            AltText = altText,
            DisplayOrder = displayOrder
        };
    }
}
