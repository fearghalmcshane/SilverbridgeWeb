using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class TeamNameChangedDomainEvent(Guid TeamId, string Name) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
    public string Name { get; } = Name;
}
