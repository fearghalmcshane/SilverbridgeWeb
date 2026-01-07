using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Events.Domain.Events;

namespace SilverbridgeWeb.Modules.Events.Application.Events.RescheduleEvent;

internal sealed class EventRescheduledDomainEventHandler : DomainEventHandler<EventRescheduledDomainEvent>
{
    public override Task Handle(EventRescheduledDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
