using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class TeamCoachUpdatedDomainEvent(Guid TeamId, string? CoachName) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
    public string? CoachName { get; } = CoachName;
}
