using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CancelSubscription;

/// <param name="RequestingUserId">
/// The user asking, who must own the subscription, or <see langword="null"/> when an administrator or
/// another module asked.
/// </param>
public sealed record CancelSubscriptionCommand(Guid SubscriptionId, Guid? RequestingUserId) : ICommand;
