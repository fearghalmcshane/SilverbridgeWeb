namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;

/// <summary>A verified provider callback waiting to be applied, kept afterwards as an audit trail.</summary>
internal sealed class ProviderWebhookEvent
{
    public string Id { get; init; }

    public string Type { get; init; }

    public string Payload { get; init; }

    public DateTime ReceivedAtUtc { get; init; }

    public DateTime? ProcessedAtUtc { get; init; }

    public int AttemptCount { get; init; }

    public DateTime? NextAttemptAtUtc { get; init; }

    public string? Error { get; init; }
}
