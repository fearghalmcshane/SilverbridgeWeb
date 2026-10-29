using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

public static class SubscriptionErrors
{
    public static Error NotFound(Guid subscriptionId) =>
        Error.NotFound("Subscriptions.NotFound", $"The subscription with the identifier {subscriptionId} was not found");

    public static Error NotFoundForProvider(string providerSubscriptionId) =>
        Error.NotFound("Subscriptions.NotFound", $"No subscription matches the provider subscription {providerSubscriptionId}");

    public static readonly Error NotASubscription = Error.Problem(
        "Subscriptions.NotASubscription",
        "Only monthly payment requests can start a subscription");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "Subscriptions.AlreadyCancelled",
        "The subscription is already cancelled");
}
