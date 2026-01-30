using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class TeamCreatedDomainEvent(Guid TeamId, string Name, AgeGroup AgeGroup, SportType SportType) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
    public string Name { get; } = Name;
    public AgeGroup AgeGroup { get; } = AgeGroup;
    public SportType SportType { get; } = SportType;
}
