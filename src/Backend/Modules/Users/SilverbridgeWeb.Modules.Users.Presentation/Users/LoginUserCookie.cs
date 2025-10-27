using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.ApiResults;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Users.Domain.Users;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class LoginUserCookie : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login-cookie", async (
            Request request,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            HttpContext httpContext) =>
        {
            User? user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Results.Problem(
                    title: "Login failed",
                    detail: "Invalid email or password",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            SignInResult result = await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return Results.Problem(
                    title: "Login failed",
                    detail: "Invalid email or password",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            IList<string> roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = request.RememberMe,
                    ExpiresUtc = request.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                });

            return Results.Ok(new
            {
                UserId = user.Id,
                user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles
            });
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public bool RememberMe { get; init; }
    }
}
