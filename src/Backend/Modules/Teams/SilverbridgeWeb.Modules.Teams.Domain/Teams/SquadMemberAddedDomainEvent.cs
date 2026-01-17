using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class SquadMemberAddedDomainEvent(Guid TeamId, Guid UserId, string FirstName, string LastName) : DomainEvent
{
    public Guid TeamId { get; } = TeamId;
    public Guid UserId { get; } = UserId;
    public string FirstName { get; } = FirstName;
    public string LastName { get; } = LastName;
}
