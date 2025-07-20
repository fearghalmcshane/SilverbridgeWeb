using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Ticketing.Domain.Events;
using SilverbridgeWeb.Modules.Ticketing.Domain.Tickets;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Ticketing.Infrastructure.Tickets;

internal sealed class TicketRepository(TicketingDbContext context) : ITicketRepository
{
    public async Task<IEnumerable<Ticket>> GetForEventAsync(
        Event @event,
        CancellationToken cancellationToken = default)
    {
        return await context.Tickets.Where(t => t.EventId == @event.Id).ToListAsync(cancellationToken);
    }

    public void InsertRange(IEnumerable<Ticket> tickets)
    {
        context.Tickets.AddRange(tickets);
    }
}
