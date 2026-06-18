using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.Common.Application.EventBus;

namespace SilverbridgeWeb.Common.Infrastructure.Eventbus;

internal sealed class EventBus(IServiceScopeFactory serviceScopeFactory) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();

        IEnumerable<IIntegrationEventConsumer<T>> consumers =
            scope.ServiceProvider.GetServices<IIntegrationEventConsumer<T>>();

        foreach (IIntegrationEventConsumer<T> consumer in consumers)
        {
            await consumer.ConsumeAsync(integrationEvent, cancellationToken);
        }
    }
}
