using MediatR;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Ticketing.Application.Payments.RefundPaymentsForEvent;
using SilverbridgeWeb.Modules.Ticketing.Domain.Events;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Events.CancelEvent;

internal sealed class RefundPaymentsEventCanceledDomainEventHandler(ISender sender)
    : DomainEventHandler<EventCanceledDomainEvent>
{
    public override async Task Handle(
        EventCanceledDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(new RefundPaymentsForEventCommand(domainEvent.EventId), cancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(RefundPaymentsForEventCommand), result.Error);
        }
    }
}
