using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Payments.Application.Webhooks.ReceiveProviderWebhook;

namespace SilverbridgeWeb.Modules.Payments.Presentation.Webhooks;

/// <summary>
/// Receives Stripe callbacks relayed by the WebUI. The event is only verified and stored here; a background job
/// applies it, so Stripe is answered quickly and retries anything that was not stored.
/// </summary>
internal sealed class StripeWebhook : IEndpoint
{
    internal const string SignatureHeader = "Stripe-Signature";
    private const int MaxRequestBodySize = 1024 * 1024;
    private static readonly UTF8Encoding s_strictUtf8 = new(false, true);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "payments/webhooks/stripe",
                async (HttpContext context, ISender sender, ILogger<StripeWebhook> logger) =>
                {
                    byte[]? body = await ReadBoundedAsync(context.Request.Body, context.RequestAborted);
                    if (body is null)
                    {
                        return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
                    }

                    string payload;
                    try
                    {
                        payload = s_strictUtf8.GetString(body);
                    }
                    catch (DecoderFallbackException exception)
                    {
                        logger.LogWarning(exception, "Rejected Stripe webhook with invalid UTF-8.");
                        return Results.BadRequest();
                    }

                    string? signature = context.Request.Headers[SignatureHeader].Count == 1
                        ? context.Request.Headers[SignatureHeader].ToString()
                        : null;

                    Result result = await sender.Send(
                        new ReceiveProviderWebhookCommand(payload, signature),
                        context.RequestAborted);

                    if (result.IsFailure)
                    {
                        logger.LogWarning("Rejected Stripe webhook: {Reason}", result.Error.Description);
                        return Results.BadRequest();
                    }

                    return Results.Ok();
                })
            .AllowAnonymous()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
            .WithTags(Tags.Payments);
    }

    private static async Task<byte[]?> ReadBoundedAsync(Stream requestBody, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[8192];
        while (true)
        {
            int read = await requestBody.ReadAsync(chunk, cancellationToken);
            if (read == 0)
            {
                return buffer.ToArray();
            }

            if (buffer.Length + read > MaxRequestBodySize)
            {
                return null;
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
    }
}
