using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.News.IntegrationEvents;

public sealed class ArticlePublishedIntegrationEvent : IntegrationEvent
{
    public ArticlePublishedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid articleId,
        string title,
        string summary,
        string slug,
        Guid categoryId,
        DateTime publishedAtUtc)
        : base(id, occurredOnUtc)
    {
        ArticleId = articleId;
        Title = title;
        Summary = summary;
        Slug = slug;
        CategoryId = categoryId;
        PublishedAtUtc = publishedAtUtc;
    }

    public Guid ArticleId { get; init; }

    public string Title { get; init; }

    public string Summary { get; init; }

    public string Slug { get; init; }

    public Guid CategoryId { get; init; }

    public DateTime PublishedAtUtc { get; init; }
}
