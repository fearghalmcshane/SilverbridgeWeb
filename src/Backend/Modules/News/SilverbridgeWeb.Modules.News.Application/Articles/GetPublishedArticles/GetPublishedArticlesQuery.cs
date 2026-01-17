using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetPublishedArticles;

public sealed record GetPublishedArticlesQuery(
    int Page = 0,
    int PageSize = 10) : IQuery<GetPublishedArticlesResponse>;
