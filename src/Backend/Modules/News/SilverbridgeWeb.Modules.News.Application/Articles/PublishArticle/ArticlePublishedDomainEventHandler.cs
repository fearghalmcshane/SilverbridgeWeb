using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.IntegrationEvents;

namespace SilverbridgeWeb.Modules.News.Application.Articles.PublishArticle;

internal sealed class ArticlePublishedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<ArticlePublishedDomainEvent>
{
    public override async Task Handle(
        ArticlePublishedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new ArticlePublishedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.ArticleId,
                domainEvent.Title,
                domainEvent.Summary,
                domainEvent.Slug,
                domainEvent.CategoryId,
                domainEvent.PublishedAtUtc),
            cancellationToken);
    }
}
