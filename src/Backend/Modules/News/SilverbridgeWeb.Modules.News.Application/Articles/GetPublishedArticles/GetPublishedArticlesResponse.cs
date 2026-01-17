namespace SilverbridgeWeb.Modules.News.Application.Articles.GetPublishedArticles;

public sealed record ArticleListItemResponse(
    Guid Id,
    string Title,
    string? Excerpt,
    string? FeaturedImage,
    Guid AuthorId,
    DateTime PublishedAtUtc,
    IReadOnlyCollection<Guid> CategoryIds);

public sealed record GetPublishedArticlesResponse(
    IReadOnlyCollection<ArticleListItemResponse> Articles,
    int TotalCount,
    int Page,
    int PageSize);
