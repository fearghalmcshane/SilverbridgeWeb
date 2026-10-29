namespace SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Incomplete = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3
}
