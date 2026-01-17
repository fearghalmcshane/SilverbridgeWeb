using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticlePublishedDomainEvent(
    Guid ArticleId,
    string Title,
    Guid AuthorId,
    DateTime PublishedAtUtc) : DomainEvent
{
    public Guid ArticleId { get; } = ArticleId;
    public string Title { get; } = Title;
    public Guid AuthorId { get; } = AuthorId;
    public DateTime PublishedAtUtc { get; } = PublishedAtUtc;
}
