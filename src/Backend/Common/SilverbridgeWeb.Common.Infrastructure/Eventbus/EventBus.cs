using MassTransit;
using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Common.Infrastructure.Eventbus;

internal sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        await bus.Publish(integrationEvent, cancellationToken);
    }
}
