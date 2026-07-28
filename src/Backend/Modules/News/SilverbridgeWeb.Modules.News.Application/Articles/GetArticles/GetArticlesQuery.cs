using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticles;

public sealed record GetArticlesQuery(
    int Page,
    int PageSize,
    Guid? CategoryId,
    ArticleStatus? Status,
    bool IncludeAllStatuses = false) : IQuery<GetArticlesResponse>;
