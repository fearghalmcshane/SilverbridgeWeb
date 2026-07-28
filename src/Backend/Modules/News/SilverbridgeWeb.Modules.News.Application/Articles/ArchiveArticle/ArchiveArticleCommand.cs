using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.ArchiveArticle;

public sealed record ArchiveArticleCommand(Guid ArticleId) : ICommand;
