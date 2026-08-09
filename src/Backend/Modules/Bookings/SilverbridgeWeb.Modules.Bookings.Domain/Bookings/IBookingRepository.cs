namespace SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Booking>> GetForRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Booking>> GetForFacilityRangeAsync(
        Guid facilityId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        Guid facilityId,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        Guid? excludedBookingId = null,
        CancellationToken cancellationToken = default);

    Task<bool> TryInsertAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<bool> TryInsertRangeAsync(
        IReadOnlyCollection<Booking> bookings,
        CancellationToken cancellationToken = default);

    Task<bool> TryUpdateAsync(Booking booking, CancellationToken cancellationToken = default);
}
