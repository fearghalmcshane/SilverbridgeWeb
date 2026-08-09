using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Bookings.Application.Bookings.DeleteBooking;

namespace SilverbridgeWeb.Modules.Bookings.Presentation.Bookings;

internal sealed class DeleteBooking : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("bookings/{id:guid}", async (Guid id, ISender sender) =>
        {
            Result result = await sender.Send(new DeleteBookingCommand(id));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.DeleteBookings)
        .WithTags(Tags.Bookings);
    }
}
