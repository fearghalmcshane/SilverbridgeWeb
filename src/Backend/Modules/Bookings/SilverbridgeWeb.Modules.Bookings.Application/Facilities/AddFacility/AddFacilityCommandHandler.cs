using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

namespace SilverbridgeWeb.Modules.Bookings.Application.Facilities.AddFacility;

internal sealed class AddFacilityCommandHandler(
    IFacilityRepository facilityRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddFacilityCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = Facility.Create(request.Name, request.Description, request.Color);

        facilityRepository.Insert(facility);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return facility.Id;
    }
}
