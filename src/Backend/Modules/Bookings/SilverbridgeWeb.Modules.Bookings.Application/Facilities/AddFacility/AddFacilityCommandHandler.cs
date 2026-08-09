using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Application.Facilities.AddFacility;

internal sealed class AddFacilityCommandHandler(IFacilityRepository facilityRepository)
    : ICommandHandler<AddFacilityCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddFacilityCommand request, CancellationToken cancellationToken)
    {
        string name = request.Name.Trim();
        if (await facilityRepository.ExistsWithNameAsync(name, cancellationToken))
        {
            return Result.Failure<Guid>(FacilityErrors.NameNotUnique);
        }

        var facility = Facility.Create(name, request.Description.Trim(), request.Color);

        if (!await facilityRepository.TryInsertAsync(facility, cancellationToken))
        {
            return Result.Failure<Guid>(FacilityErrors.NameNotUnique);
        }

        return facility.Id;
    }
}
