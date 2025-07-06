using Microsoft.AspNetCore.Routing;

namespace SilverbridgeWeb.Modules.Events.Presentation.Events;

public static class EventEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        CreateEvent.MapEndpoint(app);
        GetEvent.MapEndpoint(app);
    }
}
