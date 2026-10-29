using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

public static class ProviderErrors
{
    public static Error RequestFailed(string operation) =>
        Error.Failure("Payments.ProviderRequestFailed", $"The payment provider could not {operation}");

    public static readonly Error InvalidSignature = Error.Problem(
        "Payments.InvalidWebhookSignature",
        "The webhook signature is missing or invalid");
}
