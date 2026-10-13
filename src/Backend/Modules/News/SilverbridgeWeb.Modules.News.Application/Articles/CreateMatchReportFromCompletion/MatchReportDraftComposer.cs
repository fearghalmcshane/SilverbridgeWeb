using System.Globalization;
using System.Text;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;

internal sealed record MatchReportDraft(
    string Title,
    string Slug,
    string Summary,
    string Content,
    string HomeTeam,
    int HomeGoals,
    int HomePoints,
    string AwayTeam,
    int AwayGoals,
    int AwayPoints);

/// <summary>
/// Builds the generated text and home/away layout of a draft match report. Pure so it can be unit tested.
/// </summary>
internal static class MatchReportDraftComposer
{
    internal const int MaxTitleLength = 300;
    internal const int MaxSlugLength = 200;
    internal const int MaxSummaryLength = 1000;

    // Leaves room for a "-NN" collision suffix without exceeding the slug column limit.
    private const int MaxBaseSlugLength = MaxSlugLength - 4;

    public static MatchReportDraft Compose(CreateMatchReportFromCompletionCommand command)
    {
        bool teamIsHome = !string.Equals(command.VenueType, "away", StringComparison.OrdinalIgnoreCase);

        (string homeTeam, int homeGoals, int homePoints, string homeScore) = teamIsHome
            ? (command.TeamName, command.TeamGoals, command.TeamPoints, command.TeamScoreDisplay)
            : (command.OpponentName, command.OpponentGoals, command.OpponentPoints, command.OpponentScoreDisplay);
        (string awayTeam, int awayGoals, int awayPoints, string awayScore) = teamIsHome
            ? (command.OpponentName, command.OpponentGoals, command.OpponentPoints, command.OpponentScoreDisplay)
            : (command.TeamName, command.TeamGoals, command.TeamPoints, command.TeamScoreDisplay);

        string scoreLine = $"{homeTeam} {homeScore} – {awayScore} {awayTeam}";
        string matchDate = command.EndedAtUtc.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
        string? competition = CompetitionLabel(command.Competition, command.Stage);

        var summary = new StringBuilder(scoreLine);
        if (competition is not null)
        {
            summary.Append(" · ").Append(competition);
        }

        if (!string.IsNullOrWhiteSpace(command.VenueName))
        {
            summary.Append(" · ").Append(command.VenueName.Trim());
        }

        summary.Append(" · ").Append(matchDate);

        var content = new StringBuilder();
        content.Append("Final score: ").Append(scoreLine).Append('.');
        if (competition is not null)
        {
            content.AppendLine().Append("Competition: ").Append(competition).Append('.');
        }

        if (!string.IsNullOrWhiteSpace(command.VenueName))
        {
            content.AppendLine().Append("Venue: ").Append(command.VenueName.Trim()).Append('.');
        }

        content.AppendLine().Append("Date: ").Append(matchDate).Append('.');
        content.AppendLine().AppendLine()
            .Append("This report was generated automatically from Team Tally. Add match commentary before publishing.");

        return new MatchReportDraft(
            Truncate(Headline(command, competition), MaxTitleLength),
            Slug(command.TeamName, command.OpponentName, command.EndedAtUtc),
            Truncate(summary.ToString(), MaxSummaryLength),
            content.ToString(),
            homeTeam,
            homeGoals,
            homePoints,
            awayTeam,
            awayGoals,
            awayPoints);
    }

    public static string WithSlugSuffix(string slug, int attempt) =>
        attempt <= 1 ? slug : $"{slug}-{attempt.ToString(CultureInfo.InvariantCulture)}";

    private static string Headline(CreateMatchReportFromCompletionCommand command, string? competition)
    {
        string team = command.TeamName.Trim();
        string opponent = command.OpponentName.Trim();
        string headline = command.Result.Trim().ToUpperInvariant() switch
        {
            "WIN" or "WON" => $"{team} beat {opponent}",
            "LOSS" or "LOSE" or "LOST" => $"{team} lose to {opponent}",
            "DRAW" or "DREW" => $"{team} draw with {opponent}",
            _ => $"{team} v {opponent}"
        };

        return competition is null ? headline : $"{headline} in {competition}";
    }

    private static string? CompetitionLabel(string? competition, string? stage)
    {
        string? trimmedCompetition = string.IsNullOrWhiteSpace(competition) ? null : competition.Trim();
        string? trimmedStage = string.IsNullOrWhiteSpace(stage) ? null : stage.Trim();

        return (trimmedCompetition, trimmedStage) switch
        {
            (null, null) => null,
            (not null, null) => trimmedCompetition,
            (null, not null) => trimmedStage,
            _ => $"{trimmedCompetition} {trimmedStage}"
        };
    }

    private static string Slug(string team, string opponent, DateTime endedAtUtc)
    {
        string date = endedAtUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string teamSlug = Slugify(team);
        string opponentSlug = Slugify(opponent);
        string names = teamSlug.Length == 0 || opponentSlug.Length == 0
            ? "match-report"
            : $"{teamSlug}-v-{opponentSlug}";

        int maxNamesLength = MaxBaseSlugLength - date.Length - 1;
        if (names.Length > maxNamesLength)
        {
            names = names[..maxNamesLength].TrimEnd('-');
        }

        return $"{names}-{date}";
    }

    private static string Slugify(string value)
    {
        string decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        bool pendingHyphen = false;
        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            char lower = char.ToLowerInvariant(character);
            if (lower is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (pendingHyphen && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(lower);
                pendingHyphen = false;
            }
            else
            {
                pendingHyphen = true;
            }
        }

        return builder.ToString();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength].TrimEnd();
}
