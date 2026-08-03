using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

/// <summary>
/// Optional 1:1 template data for an <see cref="Article"/> of <see cref="ArticleType.MatchReport"/>.
/// Uses the owning article's identifier as its own primary key (shared PK) rather than a client-generated
/// key, so it doesn't need special EF change-tracking handling when a new instance is attached to an
/// already-tracked <see cref="Article"/> (see <see cref="Articles.ArticleRepository"/>).
/// </summary>
public sealed class MatchReportDetails : Entity
{
    private MatchReportDetails()
    {
    }

    public Guid ArticleId { get; private set; }

    public string HomeTeam { get; private set; }

    public string AwayTeam { get; private set; }

    public int HomeGoals { get; private set; }

    public int HomePoints { get; private set; }

    public int AwayGoals { get; private set; }

    public int AwayPoints { get; private set; }

    public string? Competition { get; private set; }

    public string? Venue { get; private set; }

    public DateTime? MatchDateUtc { get; private set; }

    internal static MatchReportDetails Create(
        Guid articleId,
        string homeTeam,
        string awayTeam,
        int homeGoals,
        int homePoints,
        int awayGoals,
        int awayPoints,
        string? competition,
        string? venue,
        DateTime? matchDateUtc)
    {
        return new MatchReportDetails
        {
            ArticleId = articleId,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeGoals = homeGoals,
            HomePoints = homePoints,
            AwayGoals = awayGoals,
            AwayPoints = awayPoints,
            Competition = competition,
            Venue = venue,
            MatchDateUtc = AsUtc(matchDateUtc)
        };
    }

    internal void Update(
        string homeTeam,
        string awayTeam,
        int homeGoals,
        int homePoints,
        int awayGoals,
        int awayPoints,
        string? competition,
        string? venue,
        DateTime? matchDateUtc)
    {
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        HomeGoals = homeGoals;
        HomePoints = homePoints;
        AwayGoals = awayGoals;
        AwayPoints = awayPoints;
        Competition = competition;
        Venue = venue;
        MatchDateUtc = AsUtc(matchDateUtc);
    }

    /// <summary>
    /// Npgsql requires <see cref="DateTimeKind.Utc"/> for "timestamp with time zone" columns. Client-supplied
    /// dates (e.g. from a date picker) typically arrive with <see cref="DateTimeKind.Unspecified"/>, so this
    /// coerces the kind without shifting the wall-clock value, since match dates are treated as calendar dates
    /// rather than precise instants.
    /// </summary>
    private static DateTime? AsUtc(DateTime? value)
    {
        return value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
    }
}
