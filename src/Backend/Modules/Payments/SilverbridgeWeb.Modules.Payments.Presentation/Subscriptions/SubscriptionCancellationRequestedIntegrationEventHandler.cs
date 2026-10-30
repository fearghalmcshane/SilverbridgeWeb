using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CancelSubscription;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Presentation.Subscriptions;

internal sealed class SubscriptionCancellationRequestedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<SubscriptionCancellationRequestedIntegrationEvent>
{
    public override async Task Handle(
        SubscriptionCancellationRequestedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new CancelSubscriptionCommand(integrationEvent.SubscriptionId, RequestingUserId: null),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(CancelSubscriptionCommand), result.Error);
        }
    }
}
