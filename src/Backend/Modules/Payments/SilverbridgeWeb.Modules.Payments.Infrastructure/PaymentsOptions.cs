namespace SilverbridgeWeb.Modules.Payments.Infrastructure;

/// <summary>The <c>Payments</c> configuration section. Secrets come from AppHost parameters, never module JSON files.</summary>
internal sealed class PaymentsOptions
{
    public const string SectionName = "Payments";

    /// <summary>The public address of the website, used to build the addresses Stripe returns the payer to.</summary>
    public Uri? PublicBaseUrl { get; set; }

    public StripeOptions Stripe { get; set; } = new();

    public WebhookProcessingOptions Webhooks { get; set; } = new();

    public ReconciliationOptions Reconciliation { get; set; } = new();
}

internal sealed class StripeOptions
{
    public string? SecretKey { get; set; }

    public string? WebhookSigningSecret { get; set; }
}

internal sealed class WebhookProcessingOptions
{
    public int IntervalInSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 20;

    public int MaxAttempts { get; set; } = 10;
}

internal sealed class ReconciliationOptions
{
    public int IntervalInMinutes { get; set; } = 15;

    public int BatchSize { get; set; } = 50;
}
