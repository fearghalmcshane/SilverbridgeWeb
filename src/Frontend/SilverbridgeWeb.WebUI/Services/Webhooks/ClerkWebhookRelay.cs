using System.Net;
using Microsoft.Extensions.Primitives;

namespace SilverbridgeWeb.WebUI.Services.Webhooks;

internal sealed class ClerkWebhookRelay(HttpClient httpClient)
{
    public const string PublicPath = "/webhooks/clerk";
    public const long MaxRequestBodySize = 1024 * 1024;

    private const string BackendPath = "users/webhooks/clerk";

    private static readonly string[] ForwardedHeaders =
    [
        "svix-id",
        "svix-timestamp",
        "svix-signature"
    ];

    public async Task<ClerkWebhookRelayResponse> ForwardAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage relayRequest = new(HttpMethod.Post, BackendPath)
        {
            Content = new RequestBodyContent(request.Body, request.ContentLength)
        };

        if (request.ContentType is not null)
        {
            relayRequest.Content.Headers.TryAddWithoutValidation("Content-Type", request.ContentType);
        }

        foreach (string headerName in ForwardedHeaders)
        {
            if (request.Headers.TryGetValue(headerName, out StringValues values))
            {
                relayRequest.Headers.TryAddWithoutValidation(headerName, values.ToArray());
            }
        }

        using HttpResponseMessage response = await httpClient.SendAsync(
            relayRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        byte[] responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return new ClerkWebhookRelayResponse(
            (int)response.StatusCode,
            responseBody,
            response.Content.Headers.ContentType?.ToString());
    }

    private sealed class RequestBodyContent(Stream requestBody, long? contentLength) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return requestBody.CopyToAsync(stream);
        }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
        {
            return requestBody.CopyToAsync(stream, cancellationToken);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = contentLength.GetValueOrDefault();
            return contentLength.HasValue;
        }
    }
}

internal sealed record ClerkWebhookRelayResponse(int StatusCode, byte[] Body, string? ContentType);
