using SilverbridgeWeb.Modules.Matches.Presentation;

namespace SilverbridgeWeb.Modules.Matches.UnitTests;

public sealed class TeamTallyWebhookPayloadParserTests
{
    private static readonly DateTime s_receivedAt = new(2026, 10, 11, 20, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Parse_ProviderTestPayloadHasNestedMatchAndFlatScores()
    {
        const string json =
            """
            {
              "id": "bc38b465-d135-4bd5-889c-3fc84a78479b",
              "data": {
                "match": {
                  "id": "5389703f-2944-4903-bcd6-43a315b8ee23",
                  "team": {
                    "id": "26b58f0e-14b9-4adc-ba9e-bda687521b18",
                    "name": "Silverbridge Harps", "goals": 2, "score": "2-02", "total": 8, "points": 2
                  },
                  "sport": "mens_football", "stage": "Test stage",
                  "venue": { "name": "Keeley Park", "type": "home" },
                  "result": "win", "status": "completed",
                  "summary": "Silverbridge Harps 2-02 Armagh Harps 0-01",
                  "ended_at": "2026-10-11T21:52:38.821+00:00", "live_url": null,
                  "opponent": { "name": "Armagh Harps", "goals": 0, "score": "0-01", "total": 1, "points": 1 },
                  "time_zone": "Europe/London",
                  "started_at": "2026-10-11T21:50:17.892+00:00",
                  "competition": "Championship", "scheduled_at": "2026-10-11T21:49:19.724+00:00"
                }
              },
              "type": "match.completed", "created_at": "2026-10-11T21:52:40.091Z", "api_version": "2026-10-01"
            }
            """;

        ParsedTeamTallyEvent parsed = TeamTallyWebhookPayloadParser.Parse(json, s_receivedAt);

        Assert.Equal("5389703f-2944-4903-bcd6-43a315b8ee23", parsed.Completion!.ProviderMatchId);
        Assert.Equal("mens_football", parsed.Completion.Sport);
        Assert.Equal("Silverbridge Harps", parsed.Completion.TeamName);
        Assert.Equal(new SilverbridgeWeb.Modules.Matches.Domain.Matches.MatchScoreSnapshot(2, 2, 8, "2-02"), parsed.Completion.TeamScore);
        Assert.Equal(new SilverbridgeWeb.Modules.Matches.Domain.Matches.MatchScoreSnapshot(0, 1, 1, "0-01"), parsed.Completion.OpponentScore);
        Assert.Null(parsed.Completion.LiveUrl);
        Assert.Equal(new DateTime(2026, 10, 11, 21, 52, 38, 821, DateTimeKind.Utc), parsed.Completion.EndedAtUtc);
    }

    [Fact]
    public void Parse_PingDoesNotRequireMatchData()
    {
        const string json =
            """
            {"id":"63979784-2ce8-44b8-94f6-9557e527dd59",
             "data":{"message":"Test event from Team Tally. Your endpoint is set up.",
                     "endpoint_id":"6161f545-65ef-46fd-ab07-42f337abfecb"},
             "type":"ping","created_at":"2026-10-11T20:43:10.982Z","api_version":"2026-10-01"}
            """;

        ParsedTeamTallyEvent parsed = TeamTallyWebhookPayloadParser.Parse(json, s_receivedAt);

        Assert.Equal("ping", parsed.EventType);
        Assert.Null(parsed.Completion);
    }

    [Fact]
    public void Parse_CompatibleApiVersionPreservesTypedFieldsAndUnknownJsonElsewhere()
    {
        const string json =
            """
            {
              "id":"evt:opaque","type":"match.completed","api_version":"2030-01-01",
              "created_at":"2026-10-11T20:00:00Z","additional":{"kept":true},
              "data":{"match":{
                "id":"match/opaque","status":"completed","sport":"hurling",
                "competition":"Senior A","stage":"Final","ended_at":"2026-10-11T19:50:00Z",
                "venue":{"name":"Páirc an Chrócaigh","type":"neutral"},
                "team":{"name":"Cúil Óg","goals":2,"points":10,"total":16,"score":"2-10"},
                "opponent":{"name":"Áth na Rí","goals":1,"points":8,"total":11,"score":"1-8"},
                "result":"win","summary":"Cúil Óg won","live_url":"https://example.test/match"
              }}
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
            "missing-score" => """{"name":"Home","goals":2,"points":8,"score":"2-8"}""",
            "negative-score" => """{"name":"Home","goals":-1,"points":8,"total":11,"score":"-1-8"}""",
            _ => """{"name":"Home","goals":2,"points":8,"total":14,"score":"2-8"}"""
        };
        string status = variant == "invalid-status" ? "scheduled" : "completed";
        string json =
            """
            {"id":"evt","type":"match.completed","api_version":"v2","created_at":"2026-10-11T20:00:00Z",
             "data":{"match":{"id":"match","status":"__STATUS__","sport":"hurling","ended_at":"2026-10-11T19:00:00Z",
             "team":__SCORE__,"opponent":{"name":"Away",
             "goals":0,"points":0,"total":0,"score":"0-0"},"result":"draw"}}}
            """.Replace("__STATUS__", status, StringComparison.Ordinal)
            .Replace("__SCORE__", score, StringComparison.Ordinal);

        Assert.Throws<TeamTallyPayloadException>(() => TeamTallyWebhookPayloadParser.Parse(json, s_receivedAt));
    }
}
