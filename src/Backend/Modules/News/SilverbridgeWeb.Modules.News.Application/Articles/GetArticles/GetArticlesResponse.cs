namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticles;

public sealed record GetArticlesResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<ArticleSummaryResponse> Articles);
