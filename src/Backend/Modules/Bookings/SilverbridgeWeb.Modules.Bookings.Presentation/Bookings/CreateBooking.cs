using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

namespace SilverbridgeWeb.Modules.Bookings.Presentation.Bookings;

internal sealed class CreateBooking : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("bookings", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateBookingCommand(
                request.FacilityId,
                request.Title,
                request.BookerName,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.IsPublic));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Bookings);
    }

    internal sealed class Request
    {
        public Guid FacilityId { get; init; }

        public string Title { get; init; }

        public string BookerName { get; init; }

        public DateTime StartsAtUtc { get; init; }

        public DateTime EndsAtUtc { get; init; }

        public bool IsPublic { get; init; }
    }
}
