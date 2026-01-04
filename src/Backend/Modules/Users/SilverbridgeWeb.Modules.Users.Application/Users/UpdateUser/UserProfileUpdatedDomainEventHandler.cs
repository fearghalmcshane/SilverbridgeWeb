using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Users.Domain.Users;
using SilverbridgeWeb.Modules.Users.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Users.Application.Users.UpdateUser;

internal sealed class UserProfileUpdatedDomainEventHandler(IEventBus eventBus)
    : IDomainEventHandler<UserProfileUpdatedDomainEvent>
{
    public async Task Handle(UserProfileUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(
            new UserProfileUpdatedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.UserId,
                domainEvent.FirstName,
                domainEvent.LastName),
            cancellationToken);
    }
}
