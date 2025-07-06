using Microsoft.AspNetCore.Routing;

namespace SilverbridgeWeb.Modules.Events.Presentation.TicketTypes;

public static class TicketTypeEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        ChangeTicketTypePrice.MapEndpoint(app);
        CreateTicketType.MapEndpoint(app);
        GetTicketType.MapEndpoint(app);
        GetTicketTypes.MapEndpoint(app);
    }
}
