using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticlePublishedDomainEvent(
    Guid articleId,
    string title,
    string summary,
    string slug,
    Guid categoryId,
    DateTime publishedAtUtc) : DomainEvent
{
    public Guid ArticleId { get; init; } = articleId;

    public string Title { get; init; } = title;

    public string Summary { get; init; } = summary;

    public string Slug { get; init; } = slug;

    public Guid CategoryId { get; init; } = categoryId;

    public DateTime PublishedAtUtc { get; init; } = publishedAtUtc;
}
