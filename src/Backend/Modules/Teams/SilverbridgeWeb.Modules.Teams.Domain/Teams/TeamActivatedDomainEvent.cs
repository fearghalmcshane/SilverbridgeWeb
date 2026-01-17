using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class TeamActivatedDomainEvent(Guid TeamId) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
}
