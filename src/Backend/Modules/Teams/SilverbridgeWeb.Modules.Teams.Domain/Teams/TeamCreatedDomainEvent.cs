using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class TeamCreatedDomainEvent(Guid TeamId, string Name, AgeGroup AgeGroup) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
    public string Name { get; } = Name;
    public AgeGroup AgeGroup { get; } = AgeGroup;
}
