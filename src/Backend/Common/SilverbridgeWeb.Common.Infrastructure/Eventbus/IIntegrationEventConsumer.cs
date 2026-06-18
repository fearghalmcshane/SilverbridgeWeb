using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Common.Infrastructure.Eventbus;

public interface IIntegrationEventConsumer<in TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    Task ConsumeAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
