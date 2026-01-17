using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

public sealed record GetArticleQuery(Guid Id, bool IncludeDrafts = false) : IQuery<ArticleResponse?>;
