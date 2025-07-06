using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Modules.Events.Application.Events.CreateEvent;
using SilverbridgeWeb.Modules.Events.Domain.Abstractions;
using SilverbridgeWeb.Modules.Events.Presentation.ApiResults;

namespace SilverbridgeWeb.Modules.Events.Presentation.Events;

internal static class CreateEvent
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("events", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateEventCommand(
                request.CategoryId,
                request.Title,
                request.Description,
                request.Location,
                request.StartsAtUtc,
                request.EndsAtUtc));

            return result.Match(Results.Ok, ApiResults.ApiResults.Problem);
        })
        .WithTags(Tags.Events);
    }

    internal sealed class Request
    {
        public Guid CategoryId { get; init; }

        public string Title { get; init; }

        public string Description { get; init; }

        public string Location { get; init; }

        public DateTime StartsAtUtc { get; init; }

        public DateTime? EndsAtUtc { get; init; }
    }
}
