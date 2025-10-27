using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using SilverbridgeWeb.WebUI;
using SilverbridgeWeb.WebUI.Services.ApiClients;
using SilverbridgeWeb.WebUI.Services.Authentication;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

// Add cookie authentication for the Blazor frontend
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".SilverbridgeWeb.WebUI.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

// Configure HttpClient with cookie forwarding to API
builder.Services.AddHttpClient<UsersApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://silverbridgeweb-api/");
})
.AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<EventsApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://silverbridgeweb-api/");
})
.AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<PingApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://silverbridgeweb-api/");
});

builder.Services.AddTransient<CookieForwardingHandler>();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

await app.RunAsync();

