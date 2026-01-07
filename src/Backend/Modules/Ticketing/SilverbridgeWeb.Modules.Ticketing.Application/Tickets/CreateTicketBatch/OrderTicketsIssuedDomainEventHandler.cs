using MediatR;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Ticketing.Application.Tickets.GetTicket;
using SilverbridgeWeb.Modules.Ticketing.Application.Tickets.GetTicketForOrder;
using SilverbridgeWeb.Modules.Ticketing.Domain.Orders;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Tickets.CreateTicketBatch;

internal sealed class OrderTicketsIssuedDomainEventHandler(ISender sender)
    : DomainEventHandler<OrderTicketsIssuedDomainEvent>
{
    public override async Task Handle(
        OrderTicketsIssuedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<TicketResponse>> result = await sender.Send(
            new GetTicketsForOrderQuery(domainEvent.OrderId), cancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(GetTicketsForOrderQuery), result.Error);
        }

        // Send ticket confirmation notification.
    }
}
