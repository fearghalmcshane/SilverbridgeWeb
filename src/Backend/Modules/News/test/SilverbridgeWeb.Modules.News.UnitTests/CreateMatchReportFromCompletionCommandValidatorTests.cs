using FluentValidation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;

namespace SilverbridgeWeb.Modules.News.UnitTests;

public sealed class CreateMatchReportFromCompletionCommandValidatorTests
{
    private readonly CreateMatchReportFromCompletionCommandValidator _validator = new();

    [Fact]
    public void Validate_AcceptsCompletionWithOptionalFieldsMissing()
    {
        ValidationResult result = _validator.Validate(
            MatchReportCommands.Completed() with { Competition = null, Stage = null, VenueName = null, VenueType = null });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsMissingIdentifiersAndNames()
    {
        ValidationResult result = _validator.Validate(
            MatchReportCommands.Completed() with { SourceMatchId = "", TeamName = " ", OpponentName = "" });

        Assert.Equal(
            ["OpponentName", "SourceMatchId", "TeamName"],
            result.Errors.Select(e => e.PropertyName).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Validate_RejectsNegativeScores()
    {
        ValidationResult result = _validator.Validate(
            MatchReportCommands.Completed() with { TeamGoals = -1, OpponentPoints = -1 });

        Assert.Equal(
            ["OpponentPoints", "TeamGoals"],
            result.Errors.Select(e => e.PropertyName).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Validate_RejectsValuesLongerThanColumns()
    {
        ValidationResult result = _validator.Validate(
            MatchReportCommands.Completed() with { VenueName = new string('v', 201), SourceProvider = new string('p', 51) });

        Assert.Equal(
            ["SourceProvider", "VenueName"],
            result.Errors.Select(e => e.PropertyName).Order(StringComparer.Ordinal));
    }
}
