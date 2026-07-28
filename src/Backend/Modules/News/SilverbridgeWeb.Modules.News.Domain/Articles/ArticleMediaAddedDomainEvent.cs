using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class ArticleMediaAddedDomainEvent(Guid articleId, Guid mediaId) : DomainEvent
{
    public Guid ArticleId { get; init; } = articleId;

    public Guid MediaId { get; init; } = mediaId;
}
