using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Bookings.Application.Facilities.AddFacility;

namespace SilverbridgeWeb.Modules.Bookings.Presentation.Facilities;

internal sealed class AddFacility : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("bookings/facilities", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new AddFacilityCommand(
                request.Name,
                request.Description,
                request.Color));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.AddFacilities)
        .WithTags(Tags.Bookings);
    }

    internal sealed class Request
    {
        public string Name { get; init; }

        public string Description { get; init; }

        public string Color { get; init; }
    }
}
