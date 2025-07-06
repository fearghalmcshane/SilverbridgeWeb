using SilverbridgeWeb.Modules.Events.Domain.Events;
using SilverbridgeWeb.Modules.Events.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Events.Infrastructure.Events;

internal sealed class EventRepository(EventsDbContext context) : IEventRepository
{
    public void Insert(Event @event)
    {
        context.Events.Add(@event);
    }
}
