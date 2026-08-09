using Microsoft.EntityFrameworkCore;
using Npgsql;
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
        return await context.Facilities.OrderBy(f => f.Name).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Facilities.AnyAsync(f => f.Name == name, cancellationToken);
    }

    public async Task<bool> TryInsertAsync(Facility facility, CancellationToken cancellationToken = default)
    {
        context.Facilities.Add(facility);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            context.Entry(facility).State = EntityState.Detached;
            return false;
        }
    }
}
