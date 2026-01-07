using MassTransit;
using MediatR;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Attendance.Application.Tickets.CreateTicket;
using SilverbridgeWeb.Modules.Ticketing.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Attendance.Presentation.Tickets;

public sealed class TicketIssuedIntegrationEventConsumer(ISender sender)
    : IConsumer<TicketIssuedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<TicketIssuedIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new CreateTicketCommand(
                context.Message.TicketId,
                context.Message.CustomerId,
                context.Message.EventId,
                context.Message.Code),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(CreateTicketCommand), result.Error);
        }
    }
}
