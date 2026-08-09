using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Bookings.Application.Facilities.GetFacilities;

namespace SilverbridgeWeb.Modules.Bookings.Presentation.Facilities;

internal sealed class GetFacilities : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("bookings/facilities", async (ISender sender) =>
        {
            Result<IReadOnlyCollection<FacilityResponse>> result = await sender.Send(new GetFacilitiesQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ViewBookings)
        .WithTags(Tags.Bookings);
    }
}
