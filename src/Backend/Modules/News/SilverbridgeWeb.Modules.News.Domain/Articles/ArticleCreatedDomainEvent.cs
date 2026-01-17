using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticleCreatedDomainEvent(Guid ArticleId, string Title, Guid AuthorId) : DomainEvent
{
    public Guid ArticleId { get; init; } = ArticleId;
    public string Title { get; init; } = Title;
    public Guid AuthorId { get; init; } = AuthorId;
}
