namespace SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

public interface IFacilityRepository
{
    Task<Facility?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Facility>> GetAllAsync(CancellationToken cancellationToken = default);

    void Insert(Facility facility);
}
