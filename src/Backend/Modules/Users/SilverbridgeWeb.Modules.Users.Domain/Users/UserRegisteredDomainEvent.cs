using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Users.Domain.Users;

public sealed class UserRegisteredDomainEvent(Guid userId) : DomainEvent
{
    public Guid UserId { get; init; } = userId;
}
