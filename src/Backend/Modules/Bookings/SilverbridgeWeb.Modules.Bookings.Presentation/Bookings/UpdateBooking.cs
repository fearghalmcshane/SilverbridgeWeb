using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Bookings.Application.Bookings.UpdateBooking;

namespace SilverbridgeWeb.Modules.Bookings.Presentation.Bookings;

internal sealed class UpdateBooking : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("bookings/{id:guid}", async (Guid id, Request request, ISender sender) =>
        {
            Result result = await sender.Send(new UpdateBookingCommand(
                id,
                request.FacilityId,
                request.Title,
                request.ContactName,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.IsPublic));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.UpdateBookings)
        .WithTags(Tags.Bookings);
    }

    internal sealed class Request
    {
        public Guid FacilityId { get; init; }

        public string Title { get; init; }

        public string ContactName { get; init; }

        public DateTime StartsAtUtc { get; init; }

        public DateTime EndsAtUtc { get; init; }

        public bool IsPublic { get; init; }
    }
}
