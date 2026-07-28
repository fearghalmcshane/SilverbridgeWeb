using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

public sealed record GetArticleQuery(Guid? ArticleId, string? Slug) : IQuery<ArticleResponse>;
