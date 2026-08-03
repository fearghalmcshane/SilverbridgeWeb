using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticles;

public sealed class ArticleSummaryResponse
{
    public Guid Id { get; init; }

    public Guid CategoryId { get; init; }

    public string CategoryName { get; init; }

    public string Title { get; init; }

    public string Slug { get; init; }

    public string Summary { get; init; }

    public ArticleStatus Status { get; init; }

    public DateTime? PublishedAtUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public string AuthorFirstName { get; init; }

    public string AuthorLastName { get; init; }

    public ArticleType ArticleType { get; init; }

    public string? FeaturedImageUrl { get; init; }

    public string? HomeTeam { get; init; }

    public string? AwayTeam { get; init; }

    public int? HomeGoals { get; init; }

    public int? HomePoints { get; init; }

    public int? AwayGoals { get; init; }

    public int? AwayPoints { get; init; }

    public string? Competition { get; init; }
}
