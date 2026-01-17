using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class TeamDeactivatedDomainEvent(Guid TeamId) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
}
