using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

public static class FacilityErrors
{
    public static readonly Error NameNotUnique = Error.Conflict(
        "Facilities.NameNotUnique",
        "A facility with the same name already exists.");

    public static Error NotFound(Guid facilityId) =>
        Error.NotFound("Facilities.NotFound", $"Facility with id '{facilityId}' was not found.");
}
