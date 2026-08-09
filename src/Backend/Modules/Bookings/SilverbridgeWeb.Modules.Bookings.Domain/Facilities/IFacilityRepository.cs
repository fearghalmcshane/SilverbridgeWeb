namespace SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

public interface IFacilityRepository
{
    Task<Facility?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Facility>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> TryInsertAsync(Facility facility, CancellationToken cancellationToken = default);
}
