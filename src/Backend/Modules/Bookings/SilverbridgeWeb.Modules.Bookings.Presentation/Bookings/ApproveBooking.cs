using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Bookings.Application.Bookings.ApproveBooking;

namespace SilverbridgeWeb.Modules.Bookings.Presentation.Bookings;

internal sealed class ApproveBooking : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("bookings/{id:guid}/approve", async (Guid id, ISender sender) =>
        {
            Result result = await sender.Send(new ApproveBookingCommand(id));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ApproveBookings)
        .WithTags(Tags.Bookings);
    }
}
