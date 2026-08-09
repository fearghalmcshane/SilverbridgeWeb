using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Bookings.Application.Bookings.GetBookings;

namespace SilverbridgeWeb.Modules.Bookings.Presentation.Bookings;

internal sealed class GetBookings : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("bookings", async (DateTime fromUtc, DateTime toUtc, ClaimsPrincipal claims, ISender sender) =>
        {
            bool includePrivateDetails =
                claims.HasClaim("permission", Permissions.ApproveBookings) ||
                claims.HasClaim("permission", Permissions.UpdateBookings);
            Result<IReadOnlyCollection<BookingResponse>> result = await sender.Send(
                new GetBookingsQuery(fromUtc, toUtc, includePrivateDetails));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ViewBookings)
        .WithTags(Tags.Bookings);
    }
}
