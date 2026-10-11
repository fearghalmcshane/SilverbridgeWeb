namespace SilverbridgeWeb.Modules.Matches.Domain.Matches;

public sealed record MatchScoreSnapshot(int Goals, int Points, int Total, string Display);

public sealed record MatchCompletionSnapshot(
    string ProviderEventId,
    string ProviderMatchId,
    string EventType,
    string ApiVersion,
    DateTime ProviderCreatedAtUtc,
    DateTime ReceivedAtUtc,
    string Status,
    string Sport,
    string? Competition,
    string? Stage,
    DateTime EndedAtUtc,
    string? VenueName,
    string? VenueType,
    string TeamName,
    MatchScoreSnapshot TeamScore,
    string OpponentName,
    MatchScoreSnapshot OpponentScore,
    string Result,
    string? Summary,
    Uri? LiveUrl);
