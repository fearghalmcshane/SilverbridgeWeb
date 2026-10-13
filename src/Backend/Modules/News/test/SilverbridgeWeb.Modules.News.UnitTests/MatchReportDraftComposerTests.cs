using SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;

namespace SilverbridgeWeb.Modules.News.UnitTests;

public sealed class MatchReportDraftComposerTests
{
    [Theory]
    [InlineData("home")]
    [InlineData("neutral")]
    [InlineData(null)]
    public void Compose_PutsTeamAtHome_UnlessVenueIsAway(string? venueType)
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { VenueType = venueType });

        Assert.Equal("Silverbridge Harps", draft.HomeTeam);
        Assert.Equal((2, 2), (draft.HomeGoals, draft.HomePoints));
        Assert.Equal("Armagh Harps", draft.AwayTeam);
        Assert.Equal((0, 1), (draft.AwayGoals, draft.AwayPoints));
        Assert.StartsWith("Silverbridge Harps 2-02 – 0-01 Armagh Harps", draft.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_PutsOpponentAtHome_WhenVenueIsAway()
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { VenueType = "Away" });

        Assert.Equal("Armagh Harps", draft.HomeTeam);
        Assert.Equal((0, 1), (draft.HomeGoals, draft.HomePoints));
        Assert.Equal("Silverbridge Harps", draft.AwayTeam);
        Assert.Equal((2, 2), (draft.AwayGoals, draft.AwayPoints));
        Assert.StartsWith("Armagh Harps 0-01 – 2-02 Silverbridge Harps", draft.Summary, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("win", "Silverbridge Harps beat Armagh Harps")]
    [InlineData("loss", "Silverbridge Harps lose to Armagh Harps")]
    [InlineData("draw", "Silverbridge Harps draw with Armagh Harps")]
    [InlineData("abandoned", "Silverbridge Harps v Armagh Harps")]
    public void Compose_BuildsHeadlineFromResult(string result, string expected)
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { Result = result, Competition = null });

        Assert.Equal(expected, draft.Title);
    }

    [Theory]
    [InlineData("Championship", null, "Silverbridge Harps beat Armagh Harps in Championship")]
    [InlineData("Championship", "Final", "Silverbridge Harps beat Armagh Harps in Championship Final")]
    [InlineData(null, "Final", "Silverbridge Harps beat Armagh Harps in Final")]
    [InlineData("  ", null, "Silverbridge Harps beat Armagh Harps")]
    public void Compose_AppendsCompetitionAndStageToHeadline(string? competition, string? stage, string expected)
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { Competition = competition, Stage = stage });

        Assert.Equal(expected, draft.Title);
    }

    [Fact]
    public void Compose_IncludesKeyFactsInSummaryAndContent()
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(MatchReportCommands.Completed());

        Assert.Equal(
            "Silverbridge Harps 2-02 – 0-01 Armagh Harps · Championship · Keeley Park · 11 October 2026",
            draft.Summary);
        Assert.Contains("Final score: Silverbridge Harps 2-02 – 0-01 Armagh Harps.", draft.Content, StringComparison.Ordinal);
        Assert.Contains("Venue: Keeley Park.", draft.Content, StringComparison.Ordinal);
        Assert.Contains("generated automatically from Team Tally", draft.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_UsesClubLocalDate_WhenMatchEndsAfterMidnightIrishTime()
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { EndedAtUtc = new DateTime(2026, 7, 11, 23, 30, 0, DateTimeKind.Utc) });

        Assert.EndsWith("12 July 2026", draft.Summary, StringComparison.Ordinal);
        Assert.Contains("Date: 12 July 2026.", draft.Content, StringComparison.Ordinal);
        Assert.Equal("silverbridge-harps-v-armagh-harps-2026-07-12", draft.Slug);
    }

    [Fact]
    public void Compose_BuildsSlugWithoutDiacritics()
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { OpponentName = "Cúil Óg (Armagh)" });

        Assert.Equal("silverbridge-harps-v-cuil-og-armagh-2026-10-11", draft.Slug);
    }

    [Fact]
    public void Compose_FallsBackToGenericSlug_WhenNamesHaveNoSlugCharacters()
    {
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { OpponentName = "??" });

        Assert.Equal("match-report-2026-10-11", draft.Slug);
    }

    [Fact]
    public void Compose_CapsTitleAndSlugLengths()
    {
        string longName = new('a', 200);
        MatchReportDraft draft = MatchReportDraftComposer.Compose(
            MatchReportCommands.Completed() with { TeamName = longName, OpponentName = longName });

        Assert.Equal(MatchReportDraftComposer.MaxTitleLength, draft.Title.Length);
        Assert.True(draft.Slug.Length <= MatchReportDraftComposer.MaxSlugLength - 4);
        Assert.EndsWith("-2026-10-11", draft.Slug, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1, "slug")]
    [InlineData(2, "slug-2")]
    [InlineData(12, "slug-12")]
    public void WithSlugSuffix_AppendsAttemptNumberAfterFirstAttempt(int attempt, string expected)
    {
        Assert.Equal(expected, MatchReportDraftComposer.WithSlugSuffix("slug", attempt));
    }
}
