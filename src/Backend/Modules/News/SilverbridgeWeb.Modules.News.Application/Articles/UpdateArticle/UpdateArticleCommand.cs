using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.UpdateArticle;

public sealed record UpdateArticleCommand(
    Guid ArticleId,
    string Title,
    string Slug,
    string Summary,
    string Content,
    Guid CategoryId) : ICommand;
