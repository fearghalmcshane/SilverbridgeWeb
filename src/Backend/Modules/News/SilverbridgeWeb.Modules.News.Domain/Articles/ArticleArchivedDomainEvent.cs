using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticleArchivedDomainEvent(Guid articleId) : DomainEvent
{
    public Guid ArticleId { get; init; } = articleId;
}
