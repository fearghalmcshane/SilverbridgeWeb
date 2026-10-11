using SilverbridgeWeb.Modules.Matches.Application.Matches.CaptureMatchCompletion;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;
using SilverbridgeWeb.Modules.Matches.Presentation;

namespace SilverbridgeWeb.Modules.Matches.UnitTests;

public sealed class CaptureMatchCompletionCommandValidatorTests
{
    [Theory]
    [InlineData("completed")]
    [InlineData("Completed")]
    [InlineData("COMPLETED")]
    public void Validate_AcceptsStatusAcceptedByParser(string status)
    {
        string json =
            """
            {"id":"evt","type":"match.completed","api_version":"v3","created_at":"2026-10-11T20:00:00Z",
             "data":{"id":"match","status":"__STATUS__","sport":"hurling","ended_at":"2026-10-11T19:00:00Z",
             "team":{"name":"Team","score":{"goals":2,"points":10,"total":16,"display":"2-10"}},
             "opponent":{"name":"Opponent","score":{"goals":1,"points":8,"total":11,"display":"1-8"}},
             "result":"win"}}
            """.Replace("__STATUS__", status, StringComparison.Ordinal);
        ParsedTeamTallyEvent parsed = TeamTallyWebhookPayloadParser.Parse(json, DateTime.UtcNow);
        var validator = new CaptureMatchCompletionCommandValidator();

        Assert.True(validator.Validate(new CaptureMatchCompletionCommand(parsed.Completion!, json)).IsValid);
    }

    [Theory]
    [InlineData("ProviderEventId", 300)]
    [InlineData("ProviderMatchId", 300)]
    [InlineData("ApiVersion", 100)]
    [InlineData("Sport", 200)]
    [InlineData("Competition", 300)]
    [InlineData("Stage", 300)]
    [InlineData("VenueName", 300)]
    [InlineData("VenueType", 100)]
    [InlineData("TeamName", 300)]
    [InlineData("OpponentName", 300)]
    [InlineData("Result", 100)]
    [InlineData("Summary", 4000)]
    [InlineData("LiveUrl", 2000)]
    [InlineData("TeamScore", 100)]
    [InlineData("OpponentScore", 100)]
    public void Validate_EnforcesPersistenceLengthBoundary(string field, int limit)
    {
        var validator = new CaptureMatchCompletionCommandValidator();
        MatchCompletionSnapshot atLimit = WithFieldLength(field, limit);
        MatchCompletionSnapshot overLimit = WithFieldLength(field, limit + 1);

        Assert.True(validator.Validate(new CaptureMatchCompletionCommand(atLimit, "{}")).IsValid);
        Assert.False(validator.Validate(new CaptureMatchCompletionCommand(overLimit, "{}")).IsValid);
    }

    private static MatchCompletionSnapshot WithFieldLength(string field, int length)
    {
        var snapshot = new MatchCompletionSnapshot(
            "evt", "match", "match.completed", "v3", DateTime.UtcNow, DateTime.UtcNow,
            "completed", "hurling", null, null, DateTime.UtcNow, null, null,
            "Team", new MatchScoreSnapshot(2, 10, 16, "2-10"),
            "Opponent", new MatchScoreSnapshot(1, 8, 11, "1-8"), "win", null, null);
        string value = new('x', length);
        return field switch
        {
            "ProviderEventId" => snapshot with { ProviderEventId = value },
            "ProviderMatchId" => snapshot with { ProviderMatchId = value },
            "ApiVersion" => snapshot with { ApiVersion = value },
            "Sport" => snapshot with { Sport = value },
            "Competition" => snapshot with { Competition = value },
            "Stage" => snapshot with { Stage = value },
            "VenueName" => snapshot with { VenueName = value },
            "VenueType" => snapshot with { VenueType = value },
            "TeamName" => snapshot with { TeamName = value },
            "OpponentName" => snapshot with { OpponentName = value },
            "Result" => snapshot with { Result = value },
            "Summary" => snapshot with { Summary = value },
            "LiveUrl" => snapshot with { LiveUrl = new Uri(value, UriKind.Relative) },
            "TeamScore" => snapshot with { TeamScore = snapshot.TeamScore with { Display = value } },
            "OpponentScore" => snapshot with { OpponentScore = snapshot.OpponentScore with { Display = value } },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
    }
}
