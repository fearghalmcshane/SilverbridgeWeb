using SilverbridgeWeb.Modules.Matches.Presentation;

namespace SilverbridgeWeb.Modules.Matches.UnitTests;

public sealed class TeamTallyWebhookPayloadParserTests
{
    private static readonly DateTime s_receivedAt = new(2026, 10, 11, 20, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Parse_CompatibleApiVersionPreservesTypedFieldsAndUnknownJsonElsewhere()
    {
        const string json =
            """
            {
              "id":"evt:opaque","type":"match.completed","api_version":"2030-01-01",
              "created_at":"2026-10-11T20:00:00Z","additional":{"kept":true},
              "data":{
                "id":"match/opaque","status":"completed","sport":"hurling",
                "competition":"Senior A","stage":"Final","ended_at":"2026-10-11T19:50:00Z",
                "venue":{"name":"Páirc an Chrócaigh","type":"neutral"},
                "team":{"name":"Cúil Óg","score":{"goals":2,"points":10,"total":16,"display":"2-10"}},
                "opponent":{"name":"Áth na Rí","score":{"goals":1,"points":8,"total":11,"display":"1-8"}},
                "result":"win","summary":"Cúil Óg won","live_url":"https://example.test/match"
              }
            }
            """;

        ParsedTeamTallyEvent parsed = TeamTallyWebhookPayloadParser.Parse(json, s_receivedAt);

        Assert.Equal("2030-01-01", parsed.ApiVersion);
        Assert.Equal("evt:opaque", parsed.EventId);
        Assert.Equal("Páirc an Chrócaigh", parsed.Completion!.VenueName);
        Assert.Equal("Cúil Óg", parsed.Completion.TeamName);
        Assert.Equal(16, parsed.Completion.TeamScore.Total);
        Assert.Equal("1-8", parsed.Completion.OpponentScore.Display);
        Assert.Equal("win", parsed.Completion.Result);
        Assert.Equal(s_receivedAt, parsed.Completion.ReceivedAtUtc);
    }

    [Fact]
    public void Parse_UnknownVerifiedEventDoesNotRequireMatchData()
    {
        const string json =
            """{"id":"evt-1","type":"result.amended","api_version":"v2","created_at":"2026-10-11T20:00:00Z"}""";

        ParsedTeamTallyEvent parsed = TeamTallyWebhookPayloadParser.Parse(json, s_receivedAt);

        Assert.Null(parsed.Completion);
        Assert.Equal("result.amended", parsed.EventType);
    }

    [Fact]
    public void Parse_RejectsMalformedJson()
    {
        Assert.ThrowsAny<System.Text.Json.JsonException>(
            () => TeamTallyWebhookPayloadParser.Parse("{", s_receivedAt));
    }

    [Theory]
    [InlineData("missing-score")]
    [InlineData("negative-score")]
    [InlineData("invalid-status")]
    public void Parse_RejectsInvalidCompletionShape(string variant)
    {
        string score = variant switch
        {
            "missing-score" => """{"goals":2,"points":8,"display":"2-8"}""",
            "negative-score" => """{"goals":-1,"points":8,"total":11,"display":"-1-8"}""",
            _ => """{"goals":2,"points":8,"total":14,"display":"2-8"}"""
        };
        string status = variant == "invalid-status" ? "scheduled" : "completed";
        string json =
            """
            {"id":"evt","type":"match.completed","api_version":"v2","created_at":"2026-10-11T20:00:00Z",
             "data":{"id":"match","status":"__STATUS__","sport":"hurling","ended_at":"2026-10-11T19:00:00Z",
             "team":{"name":"Home","score":__SCORE__},"opponent":{"name":"Away",
             "score":{"goals":0,"points":0,"total":0,"display":"0-0"}},"result":"draw"}}
            """.Replace("__STATUS__", status, StringComparison.Ordinal)
            .Replace("__SCORE__", score, StringComparison.Ordinal);

        Assert.Throws<TeamTallyPayloadException>(() => TeamTallyWebhookPayloadParser.Parse(json, s_receivedAt));
    }
}
