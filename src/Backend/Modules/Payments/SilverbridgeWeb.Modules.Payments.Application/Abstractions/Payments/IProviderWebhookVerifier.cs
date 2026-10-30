using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

public interface IProviderWebhookVerifier
{
    /// <summary>Checks the provider's signature and reads the event's identity without trusting anything else in it.</summary>
    Result<VerifiedProviderWebhook> Verify(string payload, string? signature);
}

public sealed record VerifiedProviderWebhook(string EventId, string EventType);
