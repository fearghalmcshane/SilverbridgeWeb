using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace SilverbridgeWeb.WebUI.BrowserTests;

public sealed class HarnessFixture : IDisposable
{
    private readonly WebApplication _application;

    public HarnessFixture()
    {
        string harnessPath = Path.Combine(AppContext.BaseDirectory, "Harness");
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        _application = builder.Build();

        _application.MapGet("/", () => Results.File(
            Path.Combine(harnessPath, "index.html"),
            "text/html"));
        _application.MapGet("/themeBootstrap.js", () => Results.File(
            Path.Combine(harnessPath, "themeBootstrap.js"),
            "text/javascript"));
        _application.MapGet("/themePreference.js", () => Results.File(
            Path.Combine(harnessPath, "themePreference.js"),
            "text/javascript"));

        _application.Urls.Add("http://127.0.0.1:0");
        _application.StartAsync().GetAwaiter().GetResult();

        IServer server = _application.Services.GetRequiredService<IServer>();
        Address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    }

    public string Address { get; }

    public void Dispose()
    {
        _application.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}

[CollectionDefinition(Name)]
public sealed class HarnessCollection : ICollectionFixture<HarnessFixture>
{
    public const string Name = "Static WebUI harness";
}
