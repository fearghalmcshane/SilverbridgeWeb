using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

public sealed record ArticleResponse(
    Guid Id,
    string Title,
    string Content,
    string? Excerpt,
    string? FeaturedImage,
    Guid AuthorId,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    ArticleStatus Status,
    IReadOnlyCollection<Guid> CategoryIds);
