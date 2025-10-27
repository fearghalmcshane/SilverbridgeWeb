using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using SilverbridgeWeb.WebUI.Services.ApiClients;

namespace SilverbridgeWeb.WebUI.Services.Authentication;

internal sealed class AuthenticationService
{
    private readonly UsersApiClient _usersClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationService(
        UsersApiClient usersClient,
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _usersClient = usersClient;
        _httpContextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> LoginAsync(string email, string password, bool rememberMe)
    {
        // Call API to authenticate and get user details
        LoginResponse? response = await _usersClient.LoginAsync(email, password, rememberMe);

        if (response == null)
        {
            return false;
        }

        // Create authentication session on the frontend server
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new(ClaimTypes.Email, response.Email),
                new(ClaimTypes.Name, response.FullName)
            };

            claims.AddRange(response.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                });

            // Notify Blazor that authentication state has changed
            if (_authenticationStateProvider is PersistentAuthenticationStateProvider provider)
            {
                provider.NotifyAuthenticationStateChanged();
            }
        }

        return true;
    }

    public async Task SignOutAsync()
    {
        // Call API logout
        await _usersClient.LogoutAsync();

        // Sign out locally
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Notify Blazor that authentication state has changed
            if (_authenticationStateProvider is PersistentAuthenticationStateProvider provider)
            {
                provider.NotifyAuthenticationStateChanged();
            }
        }
    }
}
