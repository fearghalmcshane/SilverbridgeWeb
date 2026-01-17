using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class SquadMemberRemovedDomainEvent(Guid TeamId, Guid UserId) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
    public Guid UserId { get; } = UserId;
}
