using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.UpdateArticle;

public sealed record UpdateArticleCommand(
    Guid ArticleId,
    string Title,
    string Content,
    string? Excerpt = null,
    string? FeaturedImage = null,
    IReadOnlyCollection<Guid>? CategoryIds = null) : ICommand;
