using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using SilverbridgeWeb.WebUI.Services.Webhooks;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class TeamTallyWebhookRelayTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task PreservesSignedBodyAndBackendResponse(HttpStatusCode statusCode)
    {
        byte[] payload = Encoding.UTF8.GetBytes("""{ "type": "match.completed", "venue": "Páirc Uí Rinn" }""");
        using var handler = new RecordingHandler(statusCode);
        using HttpClient httpClient = new(handler) { BaseAddress = new("https://silverbridgeweb-api/") };
        var relay = new TeamTallyWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(payload);
        context.Request.ContentType = "application/json";
        context.Request.Headers[TeamTallyWebhookRelay.SignatureHeader] = "t=1791127264,v1=example";
        context.Request.Headers.Authorization = "must-not-forward";
        context.Request.Headers.Cookie = "must-not-forward";
        context.Request.Headers["svix-signature"] = "must-not-forward";

        TeamTallyWebhookRelayResponse result = await relay.ForwardAsync(context.Request, CancellationToken.None);

        handler.Method.Should().Be(HttpMethod.Post);
        handler.Uri.Should().Be(new Uri("https://silverbridgeweb-api/matches/webhooks/team-tally"));
        handler.Body.Should().Equal(payload);
        handler.ContentType.Should().Be("application/json");
        handler.Headers.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new KeyValuePair<string, string>(TeamTallyWebhookRelay.SignatureHeader, "t=1791127264,v1=example"));
        result.StatusCode.Should().Be((int)statusCode);
        result.Body.Should().Equal("backend response"u8.ToArray());
        result.ContentType.Should().Be("text/plain; charset=utf-8");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RejectsOversizedBodyBeforeForwarding(bool hasContentLength)
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using HttpClient httpClient = new(handler) { BaseAddress = new("https://silverbridgeweb-api/") };
        var relay = new TeamTallyWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(new byte[TeamTallyWebhookRelay.MaxRequestBodySize + 1]);
        context.Request.ContentLength = hasContentLength ? context.Request.Body.Length : null;

        Func<Task> action = () => relay.ForwardAsync(context.Request, CancellationToken.None);

        (await action.Should().ThrowAsync<BadHttpRequestException>())
            .Which.StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);
        handler.Uri.Should().BeNull();
    }

    [Fact]
    public async Task ForwardsPayloadAtSizeLimit()
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using HttpClient httpClient = new(handler) { BaseAddress = new("https://silverbridgeweb-api/") };
        var relay = new TeamTallyWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(new byte[TeamTallyWebhookRelay.MaxRequestBodySize]);

        await relay.ForwardAsync(context.Request, CancellationToken.None);

        handler.Body.Should().HaveCount((int)TeamTallyWebhookRelay.MaxRequestBodySize);
    }

    [Fact]
    public async Task PreservesCallerCancellation()
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using HttpClient httpClient = new(handler) { BaseAddress = new("https://silverbridgeweb-api/") };
        var relay = new TeamTallyWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream("{}"u8.ToArray());
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        Func<Task> action = () => relay.ForwardAsync(context.Request, cancellation.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
        handler.Uri.Should().BeNull();
    }

    [Theory]
    [InlineData(false, HttpStatusCode.BadGateway)]
    [InlineData(true, HttpStatusCode.GatewayTimeout)]
    public async Task PublicEndpointReturnsFailureForTransportErrors(bool timeout, HttpStatusCode expectedStatus)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        using var handler = new FailingHandler(timeout);
        using HttpClient backendClient = new(handler) { BaseAddress = new("https://silverbridgeweb-api/") };
        builder.Services.AddSingleton(new TeamTallyWebhookRelay(backendClient));
        await using WebApplication app = builder.Build();
        TeamTallyWebhookRelay.MapEndpoint(app);
        await app.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            IServer server = app.Services.GetRequiredService<IServer>();
            IServerAddressesFeature addresses = server.Features.Get<IServerAddressesFeature>()
                ?? throw new InvalidOperationException("Server addresses are unavailable.");
            using HttpClient client = new() { BaseAddress = new(addresses.Addresses.Single()) };
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.PostAsync(TeamTallyWebhookRelay.PublicPath, content, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(expectedStatus);
        }
        finally
        {
            await app.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private sealed class RecordingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }

        public Uri? Uri { get; private set; }

        public byte[]? Body { get; private set; }

        public string? ContentType { get; private set; }

        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            Uri = request.RequestUri;
            HttpContent content = request.Content ?? throw new InvalidOperationException("Missing request content.");
            Body = await content.ReadAsByteArrayAsync(cancellationToken);
            ContentType = content.Headers.ContentType?.ToString();
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                Headers.Add(header.Key, string.Join(",", header.Value));
            }

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("backend response", Encoding.UTF8, "text/plain")
            };
        }
    }

    private sealed class FailingHandler(bool timeout) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(timeout
                ? new TaskCanceledException("Test backend timeout.")
                : new HttpRequestException("Test backend unavailable."));
        }
    }
}
