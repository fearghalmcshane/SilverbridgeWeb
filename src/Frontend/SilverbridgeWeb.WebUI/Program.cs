using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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

builder.Services.AddHttpClient<FoireannApiClient>(client =>
{
    client.BaseAddress = new("https+http://silverbridgeweb-api/");
});

builder.Services.AddHttpClient<BookingsApiClient>(client =>
{
    client.BaseAddress = new("https+http://silverbridgeweb-api/");
});

builder.Services.AddHttpClient<UsersApiClient>(client =>
{
    client.BaseAddress = new("https+http://silverbridgeweb-api/");
})
.AddHttpMessageHandler<AuthorizationHandler>();

const string clerkOidcScheme = "Clerk";

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(clerkOidcScheme, options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = builder.Configuration["Clerk:Authority"];
        options.ClientId = builder.Configuration["Clerk:ClientId"];
        options.ClientSecret = builder.Configuration["Clerk:ClientSecret"];
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.SaveTokens = true;
        options.TokenValidationParameters.NameClaimType = "name";
        options.MapInboundClaims = false;

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
    }, [clerkOidcScheme]);
})
.AllowAnonymous();

app.MapGet("/authentication/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return TypedResults.Redirect("/");
})
.RequireAuthorization();

await app.RunAsync().ConfigureAwait(false);
