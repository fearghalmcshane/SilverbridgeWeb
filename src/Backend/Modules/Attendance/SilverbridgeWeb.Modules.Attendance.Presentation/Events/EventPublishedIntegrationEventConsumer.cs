using MassTransit;
using MediatR;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Attendance.Application.Events;
using SilverbridgeWeb.Modules.Events.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Attendance.Presentation.Events;

public sealed class EventPublishedIntegrationEventConsumer(ISender sender)
    : IConsumer<EventPublishedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EventPublishedIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new CreateEventCommand(
                context.Message.EventId,
                context.Message.Title,
                context.Message.Description,
                context.Message.Location,
                context.Message.StartsAtUtc,
                context.Message.EndsAtUtc),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(CreateEventCommand), result.Error);
        }
    }
}
