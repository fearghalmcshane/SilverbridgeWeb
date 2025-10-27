using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SilverbridgeWeb.WebUI.Services.ApiClients;

namespace SilverbridgeWeb.WebUI.Services.Authentication;

internal sealed class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly UsersApiClient _usersClient;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public ApiAuthenticationStateProvider(UsersApiClient usersClient)
    {
        _usersClient = usersClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        CurrentUserResponse? user = await _usersClient.GetCurrentUserAsync();

        if (user == null)
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Name)
            };

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, "apiauth");
            _currentUser = new ClaimsPrincipal(identity);
        }

        return new AuthenticationState(_currentUser);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
