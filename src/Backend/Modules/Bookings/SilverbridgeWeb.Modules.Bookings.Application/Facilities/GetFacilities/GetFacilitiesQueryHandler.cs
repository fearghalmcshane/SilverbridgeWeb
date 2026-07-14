using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Application.Facilities.GetFacilities;

internal sealed class GetFacilitiesQueryHandler(IFacilityRepository facilityRepository)
    : IQueryHandler<GetFacilitiesQuery, IReadOnlyCollection<FacilityResponse>>
{
    public async Task<Result<IReadOnlyCollection<FacilityResponse>>> Handle(
        GetFacilitiesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Facility> facilities = await facilityRepository.GetAllAsync(cancellationToken);

        IReadOnlyCollection<FacilityResponse> response = facilities
            .Select(f => new FacilityResponse(f.Id, f.Name, f.Description, f.Color))
            .ToList();

        return Result.Success(response);
    }
}
