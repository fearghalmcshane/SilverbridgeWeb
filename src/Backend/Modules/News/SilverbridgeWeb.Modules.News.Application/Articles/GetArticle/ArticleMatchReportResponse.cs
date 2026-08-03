namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

public sealed class ArticleMatchReportResponse
{
    public Guid MatchReportArticleId { get; init; }

    public string HomeTeam { get; init; }

    public string AwayTeam { get; init; }

    public int HomeGoals { get; init; }

    public int HomePoints { get; init; }

    public int AwayGoals { get; init; }

    public int AwayPoints { get; init; }

    public string? Competition { get; init; }

    public string? Venue { get; init; }

    public DateTime? MatchDateUtc { get; init; }
}
