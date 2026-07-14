using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;
using SilverbridgeWeb.Modules.Bookings.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Bookings.Infrastructure.Facilities;

internal sealed class FacilityRepository(BookingsDbContext context) : IFacilityRepository
{
    public async Task<Facility?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Facilities.SingleOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Facility>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Facilities.ToListAsync(cancellationToken);
    }

    public void Insert(Facility facility)
    {
        context.Facilities.Add(facility);
    }
}
