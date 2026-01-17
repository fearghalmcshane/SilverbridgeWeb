using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticleUpdatedDomainEvent(Guid ArticleId, string Title) : DomainEvent
{
    public Guid ArticleId { get; } = ArticleId;
    public string Title { get; } = Title;
}
