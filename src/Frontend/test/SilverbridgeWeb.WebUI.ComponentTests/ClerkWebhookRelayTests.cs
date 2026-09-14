using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SilverbridgeWeb.WebUI.Services.Webhooks;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class ClerkWebhookRelayTests
{
    [Fact]
    public async Task ForwardsSignedPayloadToPrivateApi()
    {
        const string payload = """{"type":"user.created","data":{"id":"user_123"}}""";
        using var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"synced":true}""", Encoding.UTF8, "application/json")
        });
        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new("https://silverbridgeweb-api/")
        };
        var relay = new ClerkWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        context.Request.ContentType = "application/json";
        context.Request.Headers["svix-id"] = "msg_123";
        context.Request.Headers["svix-timestamp"] = "1234567890";
        context.Request.Headers["svix-signature"] = "v1,signature";
        context.Request.Headers.Authorization = "Bearer should-not-be-forwarded";
        context.Request.Headers["x-unrelated"] = "should-not-be-forwarded";

        ClerkWebhookRelayResponse result = await relay.ForwardAsync(context.Request, CancellationToken.None);

        handler.Request.Should().NotBeNull();
        handler.Request!.Method.Should().Be(HttpMethod.Post);
        handler.Request.RequestUri.Should().Be(new Uri("https://silverbridgeweb-api/users/webhooks/clerk"));
        handler.Body.Should().Equal(Encoding.UTF8.GetBytes(payload));
        handler.ContentType.Should().Be("application/json");
        handler.Headers.Should().ContainKey("svix-id").WhoseValue.Should().Equal("msg_123");
        handler.Headers.Should().ContainKey("svix-timestamp").WhoseValue.Should().Equal("1234567890");
        handler.Headers.Should().ContainKey("svix-signature").WhoseValue.Should().Equal("v1,signature");
        handler.Headers.Should().NotContainKey("Authorization");
        handler.Headers.Should().NotContainKey("x-unrelated");
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task PropagatesBackendErrorResponse()
    {
        using var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("invalid signature", Encoding.UTF8, "text/plain")
        });
        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new("https://silverbridgeweb-api/")
        };
        var relay = new ClerkWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream("{}"u8.ToArray());

        ClerkWebhookRelayResponse result = await relay.ForwardAsync(context.Request, CancellationToken.None);

        result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        result.Body.Should().Equal("invalid signature"u8.ToArray());
        result.ContentType.Should().Be("text/plain; charset=utf-8");
    }

    [Fact]
    public void UsesDedicatedPublicRoute()
    {
        ClerkWebhookRelay.PublicPath.Should().Be("/webhooks/clerk");
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public byte[]? Body { get; private set; }

        public string? ContentType { get; private set; }

        public Dictionary<string, string[]> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content is null
                ? []
                : await request.Content.ReadAsByteArrayAsync(cancellationToken);
            ContentType = request.Content?.Headers.ContentType?.ToString();

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                Headers[header.Key] = header.Value.ToArray();
            }

            return responseFactory(request);
        }
    }
}
