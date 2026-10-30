using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SilverbridgeWeb.WebUI.Services.Webhooks;

namespace SilverbridgeWeb.WebUI.ComponentTests;

public sealed class StripeWebhookRelayTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task ForwardsSignedBodyAndOnlyTheStripeSignature(HttpStatusCode statusCode)
    {
        byte[] payload = Encoding.UTF8.GetBytes("""{ "id": "evt_1", "type": "checkout.session.completed" }""");
        using var handler = new RecordingHandler(statusCode);
        using HttpClient httpClient = new(handler) { BaseAddress = new("https://silverbridgeweb-api/") };
        var relay = new StripeWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(payload);
        context.Request.ContentType = "application/json; charset=utf-8";
        context.Request.Headers[StripeWebhookRelay.SignatureHeader] = "t=1791127264,v1=example";
        context.Request.Headers.Authorization = "must-not-forward";
        context.Request.Headers.Cookie = "must-not-forward";

        StripeWebhookRelayResponse result = await relay.ForwardAsync(context.Request, CancellationToken.None);

        handler.Uri.Should().Be(new Uri("https://silverbridgeweb-api/payments/webhooks/stripe"));
        handler.Body.Should().Equal(payload);
        handler.Headers.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new KeyValuePair<string, string>(StripeWebhookRelay.SignatureHeader, "t=1791127264,v1=example"));
        result.StatusCode.Should().Be((int)statusCode);
    }

    [Fact]
    public async Task RejectsOversizedBodyBeforeForwarding()
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using HttpClient httpClient = new(handler) { BaseAddress = new("https://silverbridgeweb-api/") };
        var relay = new StripeWebhookRelay(httpClient);
        var context = new DefaultHttpContext();
        context.Request.Body = new MemoryStream(new byte[StripeWebhookRelay.MaxRequestBodySize + 1]);

        Func<Task> action = () => relay.ForwardAsync(context.Request, CancellationToken.None);

        (await action.Should().ThrowAsync<BadHttpRequestException>())
            .Which.StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);
        handler.Uri.Should().BeNull();
    }

    private sealed class RecordingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public Uri? Uri { get; private set; }

        public byte[]? Body { get; private set; }

        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri;
            HttpContent content = request.Content ?? throw new InvalidOperationException("Missing request content.");
            Body = await content.ReadAsByteArrayAsync(cancellationToken);
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                Headers.Add(header.Key, string.Join(",", header.Value));
            }

            return new HttpResponseMessage(statusCode) { Content = new StringContent(string.Empty) };
        }
    }
}
