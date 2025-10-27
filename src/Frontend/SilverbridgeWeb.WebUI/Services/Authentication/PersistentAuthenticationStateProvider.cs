using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace SilverbridgeWeb.WebUI.Services.Authentication;

internal sealed class PersistentAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    public PersistentAuthenticationStateProvider(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        // Always return true - we trust the server-side cookie
        return Task.FromResult(true);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
