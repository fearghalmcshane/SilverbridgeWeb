using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace SilverbridgeWeb.WebUI.Services.Webhooks;

internal sealed class StripeWebhookRelay(HttpClient httpClient)
{
    public const string PublicPath = "/webhooks/stripe";
    public const string SignatureHeader = "Stripe-Signature";
    public const long MaxRequestBodySize = 1024 * 1024;

    public async Task<StripeWebhookRelayResponse> ForwardAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength > MaxRequestBodySize)
        {
            throw new BadHttpRequestException("Webhook payload exceeds the size limit.", StatusCodes.Status413PayloadTooLarge);
        }

        using var body = new MemoryStream();
        byte[] buffer = new byte[16 * 1024];
        int bytesRead;
        while ((bytesRead = await request.Body.ReadAsync(buffer, cancellationToken)) != 0)
        {
            if (body.Length + bytesRead > MaxRequestBodySize)
            {
                throw new BadHttpRequestException("Webhook payload exceeds the size limit.", StatusCodes.Status413PayloadTooLarge);
            }

            await body.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        using HttpRequestMessage relayRequest = new(HttpMethod.Post, "payments/webhooks/stripe")
        {
            Content = new ByteArrayContent(body.ToArray())
        };

        if (request.ContentType is not null)
        {
            relayRequest.Content.Headers.TryAddWithoutValidation("Content-Type", request.ContentType);
        }

        if (request.Headers.TryGetValue(SignatureHeader, out StringValues signature))
        {
            relayRequest.Headers.TryAddWithoutValidation(SignatureHeader, signature.ToArray());
        }

        using HttpResponseMessage response = await httpClient.SendAsync(
            relayRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        return new StripeWebhookRelayResponse(
            (int)response.StatusCode,
            await response.Content.ReadAsByteArrayAsync(cancellationToken),
            response.Content.Headers.ContentType?.ToString());
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(PublicPath, async (
            HttpContext context,
            StripeWebhookRelay relay,
            ILogger<StripeWebhookRelay> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                StripeWebhookRelayResponse response = await relay.ForwardAsync(context.Request, cancellationToken);
                context.Response.StatusCode = response.StatusCode;
                context.Response.ContentType = response.ContentType;
                await context.Response.Body.WriteAsync(response.Body, cancellationToken);
            }
            catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                logger.LogWarning(exception, "Stripe webhook exceeded the request size limit.");
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            }
            catch (HttpRequestException exception)
            {
                logger.LogError(exception, "Stripe webhook relay could not reach the backend.");
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Stripe webhook relay timed out.");
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            }
        })
        .AllowAnonymous()
        .DisableAntiforgery()
        .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize));
    }
}

internal sealed record StripeWebhookRelayResponse(int StatusCode, byte[] Body, string? ContentType);
