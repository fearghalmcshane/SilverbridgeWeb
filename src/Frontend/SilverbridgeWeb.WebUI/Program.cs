using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using SilverbridgeWeb.WebUI;
using SilverbridgeWeb.WebUI.Authentication;
using SilverbridgeWeb.WebUI.Services.ApiClients;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddHttpContextAccessor()
    .AddTransient<AuthorizationHandler>();

builder.Services.AddHttpClient<EventsApiClient>(client =>
{
    client.BaseAddress = new("https+http://silverbridgeweb-api/");
})
.AddHttpMessageHandler<AuthorizationHandler>();

builder.Services.AddHttpClient<PingApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://silverbridgeweb-api/");
});

builder.Services.AddAuthentication("silverbridgewebAuth")
    .AddKeycloakOpenIdConnect("silverbridgewebAuth", realm: "silverbridge", "silverbridgewebAuth", options =>
    {
        options.ClientId = "silverbridgeweb-webui";
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.Scope.Add("silverbridgeweb_api.all");
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.SaveTokens = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SignOutScheme = "silverbridgewebAuth";
        options.MapInboundClaims = false;

        // For development only - disable HTTPS metadata validation
        // In production, use explicit Authority configuration instead
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorizationBuilder();
builder.Services.AddCascadingAuthenticationState();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapGet("/authentication/login", () =>
{
    return TypedResults.Challenge(new AuthenticationProperties
    {
        RedirectUri = "/"
    }, [ "silverbridgewebAuth" ] );
})
.AllowAnonymous();

app.MapGet("/authentication/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync("silverbridgewebAuth", new AuthenticationProperties
    {
        RedirectUri = "/"
    });
    return TypedResults.Redirect("/");
})
.RequireAuthorization();

await app.RunAsync().ConfigureAwait(false);
