using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticleArchivedDomainEvent(Guid ArticleId) : DomainEvent
{
    public Guid ArticleId { get; init; } = ArticleId;
}
