using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Modules.Events.Application.TicketTypes.GetTicketType;
using SilverbridgeWeb.Modules.Events.Application.TicketTypes.GetTicketTypes;
using SilverbridgeWeb.Modules.Events.Domain.Abstractions;
using SilverbridgeWeb.Modules.Events.Presentation.ApiResults;

namespace SilverbridgeWeb.Modules.Events.Presentation.TicketTypes;

internal static class GetTicketTypes
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("ticket-types", async (Guid eventId, ISender sender) =>
        {
            Result<IReadOnlyCollection<TicketTypeResponse>> result = await sender.Send(
                new GetTicketTypesQuery(eventId));

            return result.Match(Results.Ok, ApiResults.ApiResults.Problem);
        })
        .WithTags(Tags.TicketTypes);
    }
}
