using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Common.Infrastructure.Eventbus;

internal sealed class EventBus(IServiceProvider serviceProvider) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        IEnumerable<IIntegrationEventConsumer<T>> consumers =
            serviceProvider.GetServices<IIntegrationEventConsumer<T>>();

        foreach (IIntegrationEventConsumer<T> consumer in consumers)
        {
            await consumer.ConsumeAsync(integrationEvent, cancellationToken);
        }
    }
}
