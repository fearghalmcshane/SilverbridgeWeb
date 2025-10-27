using Microsoft.AspNetCore.Http;

namespace SilverbridgeWeb.WebUI.Services.Authentication;

internal sealed class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            // Forward cookies from browser to API
            string? cookies = httpContext.Request.Headers.Cookie;
            if (!string.IsNullOrEmpty(cookies))
            {
                request.Headers.Add("Cookie", cookies);
            }
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}
