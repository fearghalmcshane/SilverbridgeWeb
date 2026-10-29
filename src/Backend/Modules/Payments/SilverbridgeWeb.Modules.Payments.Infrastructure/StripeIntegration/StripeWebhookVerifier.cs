using System.Text.Json;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using Stripe;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;

/// <summary>
/// Checks the <c>Stripe-Signature</c> header and reads only the event id and type. Parsing the rest is left to
/// the background job, so an event shape this SDK version does not understand is still stored rather than lost.
/// </summary>
internal sealed class StripeWebhookVerifier(
    IOptions<PaymentsOptions> options,
    IDateTimeProvider dateTimeProvider) : IProviderWebhookVerifier
{
    private const long ToleranceInSeconds = 300;

    public Result<VerifiedProviderWebhook> Verify(string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return Result.Failure<VerifiedProviderWebhook>(ProviderErrors.InvalidSignature);
        }

        try
        {
            EventUtility.ValidateSignature(
                payload,
                signature,
                options.Value.Stripe.WebhookSigningSecret,
                ToleranceInSeconds,
                new DateTimeOffset(dateTimeProvider.UtcNow).ToUnixTimeSeconds());
        }
        catch (StripeException)
        {
            return Result.Failure<VerifiedProviderWebhook>(ProviderErrors.InvalidSignature);
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            JsonElement root = document.RootElement;
            if (root.TryGetProperty("id", out JsonElement id) && id.ValueKind == JsonValueKind.String &&
                root.TryGetProperty("type", out JsonElement type) && type.ValueKind == JsonValueKind.String)
            {
                return new VerifiedProviderWebhook(id.GetString()!, type.GetString()!);
            }
        }
        catch (JsonException)
        {
            // A correctly signed body that is not an event is still rejected below.
        }

        return Result.Failure<VerifiedProviderWebhook>(ProviderErrors.InvalidSignature);
    }
}
