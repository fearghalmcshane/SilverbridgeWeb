using Microsoft.Extensions.Primitives;

namespace SilverbridgeWeb.WebUI.Services.Webhooks;

internal sealed class ClerkWebhookRelay(HttpClient httpClient)
{
    public const string PublicPath = "/webhooks/clerk";

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
        using MemoryStream body = new();
        await request.Body.CopyToAsync(body, cancellationToken);

        using HttpRequestMessage relayRequest = new(HttpMethod.Post, BackendPath)
        {
            Content = new ByteArrayContent(body.ToArray())
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
}

internal sealed record ClerkWebhookRelayResponse(int StatusCode, byte[] Body, string? ContentType);
