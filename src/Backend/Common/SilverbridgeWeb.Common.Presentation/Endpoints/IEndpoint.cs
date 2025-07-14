using Microsoft.AspNetCore.Routing;

namespace SilverbridgeWeb.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
