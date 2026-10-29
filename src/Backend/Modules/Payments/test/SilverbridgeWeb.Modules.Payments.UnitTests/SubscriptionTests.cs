using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.UnitTests;

public sealed class SubscriptionTests
{
    private static readonly DateTime Now = new(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Start_SharesThePaymentRequestIdentifier()
    {
        PaymentRequest request = PaymentRequestTests.CreateRequest(
            PaymentRequestKind.MonthlySubscription,
            new PaymentRequestLineDetails("Monthly membership", 1, 1500));

        Subscription subscription = Subscription.Start(request, "sub_1", "cus_1", Now).Value;

        Assert.Equal(request.Id, subscription.Id);
        Assert.Equal(1500, subscription.AmountMinor);
        Assert.Equal(SubscriptionStatus.Incomplete, subscription.Status);
    }

    [Fact]
    public void Start_RejectsOneOffRequests()
    {
        PaymentRequest request = PaymentRequestTests.CreateRequest(new PaymentRequestLineDetails("Ticket", 1, 1500));

        Result<Subscription> result = Subscription.Start(request, "sub_1", "cus_1", Now);

        Assert.Equal(SubscriptionErrors.NotASubscription, result.Error);
    }

    [Fact]
    public void Activation_AndCancellation_AreAnnouncedOnce()
    {
        Subscription subscription = CreateSubscription();

        subscription.UpdateStatus(SubscriptionStatus.Active, Now.AddMonths(1), Now);
        subscription.UpdateStatus(SubscriptionStatus.PastDue, null, Now);
        subscription.UpdateStatus(SubscriptionStatus.Active, null, Now);
        subscription.UpdateStatus(SubscriptionStatus.Cancelled, null, Now);
        subscription.UpdateStatus(SubscriptionStatus.Active, null, Now);

        Assert.Single(subscription.DomainEvents.OfType<SubscriptionActivatedDomainEvent>());
        Assert.Single(subscription.DomainEvents.OfType<SubscriptionCancelledDomainEvent>());
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Equal(Now.AddMonths(1), subscription.CurrentPeriodEndUtc);
    }

    [Fact]
    public void Payments_AreRecordedOncePerInvoice_AndAFailedInvoiceCanLaterSucceed()
    {
        Subscription subscription = CreateSubscription();

        subscription.RecordPayment("in_1", 1500, SubscriptionPaymentStatus.Succeeded, Now, Now.AddMonths(1), null, Now);
        subscription.RecordPayment("in_1", 1500, SubscriptionPaymentStatus.Succeeded, Now, Now.AddMonths(1), null, Now);
        subscription.RecordPayment("in_2", 1500, SubscriptionPaymentStatus.Failed, null, null, "Declined", Now);
        subscription.RecordPayment("in_2", 1500, SubscriptionPaymentStatus.Succeeded, Now.AddMonths(1), Now.AddMonths(2), null, Now);

        Assert.Equal(2, subscription.Payments.Count);
        Assert.All(subscription.Payments, p => Assert.Equal(SubscriptionPaymentStatus.Succeeded, p.Status));
        Assert.Equal(2, subscription.DomainEvents.OfType<SubscriptionPaymentSucceededDomainEvent>().Count());
        Assert.Single(subscription.DomainEvents.OfType<SubscriptionPaymentFailedDomainEvent>());
    }

    private static Subscription CreateSubscription()
    {
        PaymentRequest request = PaymentRequestTests.CreateRequest(
            PaymentRequestKind.MonthlySubscription,
            new PaymentRequestLineDetails("Monthly membership", 1, 1500));

        return Subscription.Start(request, "sub_1", "cus_1", Now).Value;
    }
}
