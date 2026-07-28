using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticleUpdatedDomainEvent(Guid articleId) : DomainEvent
{
    public Guid ArticleId { get; init; } = articleId;
}
