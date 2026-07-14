namespace SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Booking>> GetAllAsync(CancellationToken cancellationToken = default);

    void Insert(Booking booking);
}
