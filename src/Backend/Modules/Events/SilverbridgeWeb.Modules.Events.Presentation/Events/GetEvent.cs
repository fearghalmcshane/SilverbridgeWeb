using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.ApiResults;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Events.Application.Events.GetEvent;

namespace SilverbridgeWeb.Modules.Events.Presentation.Events;

internal sealed class GetEvent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("events/{id}", async (Guid id, ISender sender) =>
        {
            Result<EventResponse> result = await sender.Send(new GetEventQuery(id));

            return result.Match(Results.Ok, Common.Presentation.ApiResults.ApiResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Events);
    }
}
