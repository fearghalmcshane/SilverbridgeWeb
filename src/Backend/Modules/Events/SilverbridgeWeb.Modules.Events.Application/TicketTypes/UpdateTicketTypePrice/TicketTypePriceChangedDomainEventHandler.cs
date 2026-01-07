using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Events.Domain.TicketTypes;
using SilverbridgeWeb.Modules.Events.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Events.Application.TicketTypes.UpdateTicketTypePrice;

internal sealed class TicketTypePriceChangedDomainEventHandler(IEventBus eventBus)
     : DomainEventHandler<TicketTypePriceChangedDomainEvent>
{
    public override async Task Handle(
        TicketTypePriceChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new TicketTypePriceChangedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.TicketTypeId,
                domainEvent.Price),
            cancellationToken);
    }
}
