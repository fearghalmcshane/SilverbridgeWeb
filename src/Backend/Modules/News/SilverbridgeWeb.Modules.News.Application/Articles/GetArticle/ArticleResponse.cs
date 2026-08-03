using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

public sealed record ArticleResponse(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid AuthorUserId,
    string AuthorFirstName,
    string AuthorLastName,
    string Title,
    string Slug,
    string Summary,
    string Content,
    ArticleType ArticleType,
    ArticleStatus Status,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc)
{
    public List<ArticleMediaResponse> Media { get; } = [];

    public ArticleMatchReportResponse? MatchReport { get; set; }
}
