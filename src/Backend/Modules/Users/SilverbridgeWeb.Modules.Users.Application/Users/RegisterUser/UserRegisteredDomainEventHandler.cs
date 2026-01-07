using MediatR;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Application.Users.GetUser;
using SilverbridgeWeb.Modules.Users.Domain.Users;
using SilverbridgeWeb.Modules.Users.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Users.Application.Users.RegisterUser;

internal sealed class UserRegisteredDomainEventHandler(ISender sender, IEventBus eventBus)
    : DomainEventHandler<UserRegisteredDomainEvent>
{
    public override async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken = default)
    {
        Result<UserResponse> result = await sender.Send(new GetUserQuery(notification.UserId), cancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(GetUserQuery), result.Error);
        }

        await eventBus.PublishAsync(
            new UserRegisteredIntegrationEvent(
                notification.Id,
                notification.OccurredOnUtc,
                result.Value.Id,
                result.Value.Email,
                result.Value.FirstName,
                result.Value.LastName),
                cancellationToken);
    }
}
