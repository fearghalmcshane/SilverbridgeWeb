namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

public sealed class ArticleMediaResponse
{
    public Guid MediaId { get; init; }

    public string BlobUrl { get; init; }

    public string MediaType { get; init; }

    public string? AltText { get; init; }

    public int DisplayOrder { get; init; }
}
