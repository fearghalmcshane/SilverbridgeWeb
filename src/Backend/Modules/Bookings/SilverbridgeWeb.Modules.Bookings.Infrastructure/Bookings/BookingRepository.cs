using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;
using SilverbridgeWeb.Modules.Bookings.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Bookings.Infrastructure.Bookings;

internal sealed class BookingRepository(BookingsDbContext context) : IBookingRepository
{
    public async Task<Booking?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings.SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Bookings.OrderBy(b => b.StartsAtUtc).ToListAsync(cancellationToken);
    }

    public void Insert(Booking booking)
    {
        context.Bookings.Add(booking);
    }
}
