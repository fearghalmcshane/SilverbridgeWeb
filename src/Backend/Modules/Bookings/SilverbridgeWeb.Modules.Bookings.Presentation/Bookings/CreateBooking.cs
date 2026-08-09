using System.Security.Claims;
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
        app.MapPost("bookings", async (Request request, ClaimsPrincipal claims, ISender sender) =>
        {
            string bookerName = claims.FindFirst("display_name")?.Value ?? string.Empty;
            Result<Guid> result = await sender.Send(new CreateBookingCommand(
                request.FacilityId,
                request.Title,
                bookerName,
                request.ContactName,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.IsPublic,
                request.IsRecurring,
                request.RecurrenceDays,
                request.RecurrenceEndDate));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.CreateBookings)
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

        public bool IsRecurring { get; init; }

        public IReadOnlyCollection<DayOfWeek> RecurrenceDays { get; init; } = [];

        public DateTime? RecurrenceEndDate { get; init; }
    }
}
