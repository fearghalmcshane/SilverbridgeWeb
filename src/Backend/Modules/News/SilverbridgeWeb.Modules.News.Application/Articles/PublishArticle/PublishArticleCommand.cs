using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.PublishArticle;

public sealed record PublishArticleCommand(Guid ArticleId) : ICommand;
