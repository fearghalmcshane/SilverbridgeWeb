using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Bookings.Application.Facilities.AddFacility;

public sealed record AddFacilityCommand(
    string Name,
    string Description,
    string Color) : ICommand<Guid>;
