using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;

public sealed record CreateArticleCommand(
    string Title,
    string Content,
    Guid AuthorId,
    string? Excerpt = null,
    string? FeaturedImage = null,
    IReadOnlyCollection<Guid>? CategoryIds = null) : ICommand<Guid>;
