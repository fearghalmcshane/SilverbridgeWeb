using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Modules.Events.Application.Events.CreateEvent;

namespace SilverbridgeWeb.Modules.Events.Presentation.Events;

internal static class CreateEvent
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("events", async (Request request, ISender sender) =>
        {
            var command = new CreateEventCommand(
                request.Title,
                request.Description,
                request.Location,
                request.StartsAtUtc,
                request.EndsAtUtc);

            Guid eventId = await sender.Send(command);

            return Results.Ok(eventId);
        })
        .WithTags(Tags.Events);
    }

    internal sealed class Request
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public DateTime StartsAtUtc { get; set; }

        public DateTime? EndsAtUtc { get; set; }
    }
}
