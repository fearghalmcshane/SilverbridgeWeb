using Microsoft.EntityFrameworkCore;
using Npgsql;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;
using SilverbridgeWeb.Modules.Bookings.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Bookings.Infrastructure.Bookings;

internal sealed class BookingRepository(BookingsDbContext context) : IBookingRepository
{
    public async Task<Booking?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Bookings.SingleOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Booking>> GetForRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .Where(b =>
                b.Status != BookingStatus.Cancelled &&
                b.StartsAtUtc < toUtc &&
                b.EndsAtUtc > fromUtc)
            .OrderBy(b => b.StartsAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Booking>> GetForFacilityRangeAsync(
        Guid facilityId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings
            .Where(b =>
                b.FacilityId == facilityId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartsAtUtc < toUtc &&
                b.EndsAtUtc > fromUtc)
            .OrderBy(b => b.StartsAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        Guid facilityId,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        Guid? excludedBookingId = null,
        CancellationToken cancellationToken = default)
    {
        return await context.Bookings.AnyAsync(
            b => b.FacilityId == facilityId &&
                 (!excludedBookingId.HasValue || b.Id != excludedBookingId.Value) &&
                 b.Status != BookingStatus.Cancelled &&
                 b.StartsAtUtc < endsAtUtc &&
                 b.EndsAtUtc > startsAtUtc,
            cancellationToken);
    }

    public async Task<bool> TryInsertAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        context.Bookings.Add(booking);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ExclusionViolation
            })
        {
            context.Entry(booking).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<bool> TryInsertRangeAsync(
        IReadOnlyCollection<Booking> bookings,
        CancellationToken cancellationToken = default)
    {
        context.Bookings.AddRange(bookings);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ExclusionViolation
            })
        {
            foreach (Booking booking in bookings)
            {
                context.Entry(booking).State = EntityState.Detached;
            }

            return false;
        }
    }

    public async Task<bool> TryUpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ExclusionViolation
            })
        {
            context.Entry(booking).State = EntityState.Detached;
            return false;
        }
    }
}
