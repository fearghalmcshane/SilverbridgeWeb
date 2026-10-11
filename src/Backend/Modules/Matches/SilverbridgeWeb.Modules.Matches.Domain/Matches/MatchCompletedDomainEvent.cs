using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Matches.Domain.Matches;

public sealed class MatchCompletedDomainEvent(Guid id, Guid completionId, MatchCompletionSnapshot snapshot)
    : DomainEvent(id, snapshot.ReceivedAtUtc)
{
    public Guid CompletionId { get; init; } = completionId;

    public MatchCompletionSnapshot Snapshot { get; init; } = snapshot;
}
