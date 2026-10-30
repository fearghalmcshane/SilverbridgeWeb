namespace SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

/// <summary>Durable store for verified provider callbacks, processed later by a background job.</summary>
public interface IProviderWebhookInbox
{
    /// <returns><see langword="false"/> when the event was already stored.</returns>
    Task<bool> AddAsync(VerifiedProviderWebhook webhook, string payload, CancellationToken cancellationToken = default);
}
