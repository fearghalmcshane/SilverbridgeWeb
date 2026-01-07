using MassTransit;
using MediatR;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Events.IntegrationEvents;
using SilverbridgeWeb.Modules.Ticketing.Application.TicketTypes.UpdateTicketTypePrice;

namespace SilverbridgeWeb.Modules.Ticketing.Presentation.TicketTypes;

public sealed class TicketTypePriceChangedIntegrationEventConsumer(ISender sender)
    : IConsumer<TicketTypePriceChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TicketTypePriceChangedIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new UpdateTicketTypePriceCommand(context.Message.TicketTypeId, context.Message.Price),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(UpdateTicketTypePriceCommand), result.Error);
        }
    }
}
