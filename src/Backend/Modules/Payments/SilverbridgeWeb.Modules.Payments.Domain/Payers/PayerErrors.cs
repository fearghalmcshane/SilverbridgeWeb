using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.Payers;

public static class PayerErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("Payers.NotFound", $"No payer is known for the user {userId}");

    public static readonly Error NoProviderCustomer = Error.Problem(
        "Payers.NoProviderCustomer",
        "There is no payment account to manage yet");
}
