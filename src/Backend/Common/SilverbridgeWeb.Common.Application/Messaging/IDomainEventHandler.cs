using MediatR;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Common.Application.Messaging;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent;
