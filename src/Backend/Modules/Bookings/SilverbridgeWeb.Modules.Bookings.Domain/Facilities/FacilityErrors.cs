using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

public static class FacilityErrors
{
    public static Error NotFound(Guid facilityId) =>
        Error.NotFound("Facilities.NotFound", $"Facility with id '{facilityId}' was not found.");
}
