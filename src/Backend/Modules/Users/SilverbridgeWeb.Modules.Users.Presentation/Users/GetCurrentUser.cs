using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Presentation.Endpoints;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class GetCurrentUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", (HttpContext httpContext) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            string? userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            string? name = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Results.Ok(new
            {
                UserId = userId,
                Email = email,
                Name = name,
                Roles = roles
            });
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }
}
