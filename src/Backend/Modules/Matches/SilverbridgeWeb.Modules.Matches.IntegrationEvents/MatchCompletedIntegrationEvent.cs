using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Modules.Matches.IntegrationEvents;

public sealed class MatchCompletedIntegrationEvent : IntegrationEvent
{
    public MatchCompletedIntegrationEvent(
        Guid id,
        DateTime occurredOnUtc,
        Guid completionId,
        MatchCompletedSnapshot snapshot)
        : base(id, occurredOnUtc)
    {
        CompletionId = completionId;
        Snapshot = snapshot;
    }

    public Guid CompletionId { get; init; }

    public MatchCompletedSnapshot Snapshot { get; init; }
}

public sealed record MatchCompletedSnapshot(
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
    MatchScore TeamScore,
    string OpponentName,
    MatchScore OpponentScore,
    string Result,
    string? Summary,
    Uri? LiveUrl);

public sealed record MatchScore(int Goals, int Points, int Total, string Display);
