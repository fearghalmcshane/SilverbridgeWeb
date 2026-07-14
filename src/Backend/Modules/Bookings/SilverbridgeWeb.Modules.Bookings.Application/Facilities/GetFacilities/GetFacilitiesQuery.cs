using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Bookings.Application.Facilities.GetFacilities;

public sealed record GetFacilitiesQuery : IQuery<IReadOnlyCollection<FacilityResponse>>;
