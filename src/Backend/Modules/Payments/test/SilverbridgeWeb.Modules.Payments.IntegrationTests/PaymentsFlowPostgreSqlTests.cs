using System.Net;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;
using SilverbridgeWeb.Modules.Payments.Application.Payers.CreatePayer;
using SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CreatePaymentRequest;
using SilverbridgeWeb.Modules.Payments.Application.Reconciliation.ReconcilePendingPayments;
using SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;
using SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CancelSubscription;
using SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CreateBillingPortalSession;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;
using SilverbridgeWeb.Modules.Payments.UnitTests;

namespace SilverbridgeWeb.Modules.Payments.IntegrationTests;

public sealed class PaymentsFlowPostgreSqlTests(PaymentsHost host) : IClassFixture<PaymentsHost>
{
    [Fact]
    public async Task OneOffPayment_FromRequestThroughWebhookToRefund()
    {
        var payerId = Guid.NewGuid();
        var paymentRequestId = Guid.NewGuid();
        Assert.True((await host.SendAsync(new CreatePayerCommand(payerId, "pat@example.test", "Pat", "Murphy"))).IsSuccess);

        // Another module asks for the payment through the integration event inbox.
        await host.ConsumeAsync(new PaymentRequestedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            paymentRequestId,
            "ticketing",
            "order-42",
            payerId,
            "County final tickets",
            "GBP",
            PaymentKind.OneOff,
            [new PaymentLineModel("Adult", 2, 1000), new PaymentLineModel("Child", 1, 500)]));
        await host.RunInboxAsync();

        PaymentRequest requested = await host.GetPaymentRequestAsync(paymentRequestId);
        Assert.Equal(PaymentRequestStatus.Pending, requested.Status);
        Assert.Equal(2500, requested.AmountMinor);
        Assert.Equal(2, requested.Lines.Count);

        // Only the payer can start checkout, and pressing Pay twice reuses the open session.
        Result<CheckoutRedirectResponse> stranger = await host.SendAsync(new StartCheckoutCommand(paymentRequestId, Guid.NewGuid()));
        Assert.Equal(ErrorType.NotFound, stranger.Error.Type);

        Result<CheckoutRedirectResponse> first = await host.SendAsync(new StartCheckoutCommand(paymentRequestId, payerId));
        Result<CheckoutRedirectResponse> second = await host.SendAsync(new StartCheckoutCommand(paymentRequestId, payerId));
        Assert.Equal(first.Value.RedirectUrl, second.Value.RedirectUrl);
        CheckoutSessionRequest session = Assert.Single(host.Provider.CreatedSessions, s => s.PaymentRequestId == paymentRequestId);
        Assert.Equal($"cus_{payerId:N}", session.ProviderCustomerId);
        Assert.Single(host.Provider.CreatedCustomers, c => c.UserId == payerId && c.Email == "pat@example.test");

        string sessionId = (await host.GetPaymentRequestAsync(paymentRequestId)).ProviderCheckoutSessionId!;
        string completed = StripeEvents.CheckoutSession($"evt_{paymentRequestId:N}", "checkout.session.completed", paymentRequestId, sessionId);

        // Signed deliveries are stored once, however often Stripe sends them; unsigned ones are refused.
        using (HttpResponseMessage response = await host.PostWebhookAsync(completed))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using (HttpResponseMessage response = await host.PostWebhookAsync(completed))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using (HttpResponseMessage response = await host.PostWebhookAsync(completed, "t=1,v1=forged"))
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        Assert.Equal(1, await host.QueryScalarAsync<int>(
            "SELECT COUNT(*) FROM payments.provider_webhook_events WHERE id = @Id",
            new { Id = $"evt_{paymentRequestId:N}" }));

        await host.RunWebhooksAsync();

        PaymentRequest paid = await host.GetPaymentRequestAsync(paymentRequestId);
        Assert.Equal(PaymentRequestStatus.Succeeded, paid.Status);
        Assert.Equal("pi_test_1", paid.ProviderPaymentIntentId);

        await host.RunOutboxAsync();
        PaymentSucceededIntegrationEvent succeeded = Assert.Single(
            host.EventBus.OfType<PaymentSucceededIntegrationEvent>(), e => e.PaymentRequestId == paymentRequestId);
        Assert.Equal("ticketing", succeeded.Source);
        Assert.Equal("order-42", succeeded.SourceReference);
        Assert.Equal(2500, succeeded.AmountMinor);

        // Refunds cannot exceed what was paid; a partial refund is recorded and announced.
        Result<Guid> tooMuch = await host.SendAsync(new RefundPaymentCommand(paymentRequestId, Guid.NewGuid(), 3000, "Too much", Guid.NewGuid()));
        Assert.Equal(PaymentRequestErrors.InvalidRefundAmount, tooMuch.Error);

        var refundCommand = new RefundPaymentCommand(paymentRequestId, Guid.NewGuid(), 1000, "One ticket returned", Guid.NewGuid());
        Result<Guid> refund = await host.SendAsync(refundCommand);
        Assert.True(refund.IsSuccess);

        // Retrying the same refund, for example after a lost response, does not refund again.
        Result<Guid> retried = await host.SendAsync(refundCommand);
        Assert.Equal(refund.Value, retried.Value);

        // Stripe confirms the same refund by webhook; it is not counted twice.
        ProviderRefundRequest providerRefund = Assert.Single(host.Provider.CreatedRefunds, r => r.PaymentRequestId == paymentRequestId);
        string providerRefundId = (await host.GetPaymentRequestAsync(paymentRequestId)).Refunds.Single().ProviderRefundId!;
        using (HttpResponseMessage response = await host.PostWebhookAsync(StripeEvents.Refund(
                   $"evt_refund_{paymentRequestId:N}", "refund.updated", providerRefundId, 1000, "succeeded",
                   paymentRequestId, providerRefund.RefundId)))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        await host.RunWebhooksAsync();

        PaymentRequest refunded = await host.GetPaymentRequestAsync(paymentRequestId);
        Assert.Equal(1000, refunded.AmountRefundedMinor);
        Assert.Equal(PaymentRequestStatus.Succeeded, refunded.Status);
        Assert.Equal(RefundStatus.Succeeded, Assert.Single(refunded.Refunds).Status);

        await host.RunOutboxAsync();
        PaymentRefundedIntegrationEvent refundedEvent = Assert.Single(
            host.EventBus.OfType<PaymentRefundedIntegrationEvent>(), e => e.PaymentRequestId == paymentRequestId);
        Assert.Equal(1000, refundedEvent.AmountRefundedMinor);
        Assert.False(refundedEvent.IsFullyRefunded);
    }

    [Fact]
    public async Task UnpaidCheckouts_ExpireByWebhookOrAreReconciled()
    {
        var payerId = Guid.NewGuid();
        Guid expiring = await CreateRequestAsync(payerId, PaymentRequestKind.OneOff);
        Guid missedWebhook = await CreateRequestAsync(payerId, PaymentRequestKind.OneOff);
        await host.SendAsync(new StartCheckoutCommand(expiring, payerId));
        await host.SendAsync(new StartCheckoutCommand(missedWebhook, payerId));

        string expiringSession = (await host.GetPaymentRequestAsync(expiring)).ProviderCheckoutSessionId!;
        using (HttpResponseMessage response = await host.PostWebhookAsync(StripeEvents.CheckoutSession(
                   $"evt_expired_{expiring:N}", "checkout.session.expired", expiring, expiringSession, "unpaid", "expired")))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        await host.RunWebhooksAsync();
        Assert.Equal(PaymentRequestStatus.Expired, (await host.GetPaymentRequestAsync(expiring)).Status);

        await host.RunOutboxAsync();
        Assert.Single(host.EventBus.OfType<PaymentExpiredIntegrationEvent>(), e => e.PaymentRequestId == expiring);

        // The payer paid, but the webhook never arrived: reconciliation asks Stripe once the checkout should be over.
        string paidSession = (await host.GetPaymentRequestAsync(missedWebhook)).ProviderCheckoutSessionId!;
        host.Provider.Sessions[paidSession] = new CheckoutSessionSnapshot(
            paidSession, CheckoutSessionState.Complete, true, "pi_reconciled", null, null);
        await host.ExecuteAsync(
            """
            UPDATE payments.payment_requests
            SET checkout_expires_at_utc = now() - interval '1 hour', updated_at_utc = now() - interval '1 hour'
            WHERE id = @Id
            """,
            new { Id = missedWebhook });

        Result<int> reconciled = await host.SendAsync(new ReconcilePendingPaymentsCommand(50));

        Assert.True(reconciled.Value >= 1);
        PaymentRequest settled = await host.GetPaymentRequestAsync(missedWebhook);
        Assert.Equal(PaymentRequestStatus.Succeeded, settled.Status);
        Assert.Equal("pi_reconciled", settled.ProviderPaymentIntentId);
    }

    [Fact]
    public async Task EventsForThingsNotRecordedYet_AreRetriedLater()
    {
        string eventId = $"evt_early_{Guid.NewGuid():N}";
        using (HttpResponseMessage response = await host.PostWebhookAsync(
                   StripeEvents.Invoice(eventId, "invoice.paid", "in_early", $"sub_unknown_{Guid.NewGuid():N}", 1500)))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        await host.RunWebhooksAsync();

        Assert.Equal(1, await host.QueryScalarAsync<int>(
            "SELECT attempt_count FROM payments.provider_webhook_events WHERE id = @Id AND processed_at_utc IS NULL AND next_attempt_at_utc > now()",
            new { Id = eventId }));
    }

    [Fact]
    public async Task MonthlyDirectDebit_SetsUpCollectsAndCancels()
    {
        var payerId = Guid.NewGuid();
        Guid paymentRequestId = await CreateRequestAsync(payerId, PaymentRequestKind.MonthlySubscription);
        await host.SendAsync(new StartCheckoutCommand(paymentRequestId, payerId));
        string sessionId = (await host.GetPaymentRequestAsync(paymentRequestId)).ProviderCheckoutSessionId!;
        string providerSubscriptionId = $"sub_{paymentRequestId:N}";

        // Bacs Direct Debit: checkout finishes before the money is collected.
        await PostAndApplyAsync(StripeEvents.CheckoutSession(
            $"evt_dd_{paymentRequestId:N}", "checkout.session.completed", paymentRequestId, sessionId, "unpaid", subscriptionId: providerSubscriptionId));
        Assert.Equal(PaymentRequestStatus.Processing, (await host.GetPaymentRequestAsync(paymentRequestId)).Status);
        Assert.Equal(SubscriptionStatus.Incomplete, (await host.GetSubscriptionAsync(paymentRequestId))!.Status);

        await PostAndApplyAsync(StripeEvents.Subscription(
            $"evt_active_{paymentRequestId:N}", "customer.subscription.updated", providerSubscriptionId, "active", paymentRequestId));
        await PostAndApplyAsync(StripeEvents.Invoice(
            $"evt_invoice_{paymentRequestId:N}", "invoice.paid", $"in_{paymentRequestId:N}", providerSubscriptionId, 1500));
        await PostAndApplyAsync(StripeEvents.CheckoutSession(
            $"evt_collected_{paymentRequestId:N}", "checkout.session.async_payment_succeeded", paymentRequestId, sessionId));

        Subscription active = (await host.GetSubscriptionAsync(paymentRequestId))!;
        Assert.Equal(SubscriptionStatus.Active, active.Status);
        Assert.NotNull(active.CurrentPeriodEndUtc);
        Assert.Single(active.Payments);
        Assert.Equal(PaymentRequestStatus.Succeeded, (await host.GetPaymentRequestAsync(paymentRequestId)).Status);

        Result notOwner = await host.SendAsync(new CancelSubscriptionCommand(paymentRequestId, Guid.NewGuid()));
        Assert.Equal(ErrorType.NotFound, notOwner.Error.Type);

        Assert.True((await host.SendAsync(new CancelSubscriptionCommand(paymentRequestId, payerId))).IsSuccess);
        Assert.Contains(providerSubscriptionId, host.Provider.CancelledSubscriptions);
        Assert.Equal(SubscriptionStatus.Cancelled, (await host.GetSubscriptionAsync(paymentRequestId))!.Status);

        await host.RunOutboxAsync();
        Assert.Single(host.EventBus.OfType<SubscriptionActivatedIntegrationEvent>(), e => e.SubscriptionId == paymentRequestId);
        Assert.Single(host.EventBus.OfType<SubscriptionPaymentSucceededIntegrationEvent>(), e => e.SubscriptionId == paymentRequestId);
        Assert.Single(host.EventBus.OfType<SubscriptionCancelledIntegrationEvent>(), e => e.SubscriptionId == paymentRequestId);
        Assert.Single(host.EventBus.OfType<PaymentSucceededIntegrationEvent>(), e => e.PaymentRequestId == paymentRequestId);
    }

    [Fact]
    public async Task DirectDebitRefused_WithoutAWebhook_IsReconciledAsFailed()
    {
        var payerId = Guid.NewGuid();
        Guid paymentRequestId = await CreateRequestAsync(payerId, PaymentRequestKind.OneOff);
        await host.SendAsync(new StartCheckoutCommand(paymentRequestId, payerId));
        string sessionId = (await host.GetPaymentRequestAsync(paymentRequestId)).ProviderCheckoutSessionId!;

        await PostAndApplyAsync(StripeEvents.CheckoutSession(
            $"evt_dd_once_{paymentRequestId:N}", "checkout.session.completed", paymentRequestId, sessionId, "unpaid"));
        Assert.Equal(PaymentRequestStatus.Processing, (await host.GetPaymentRequestAsync(paymentRequestId)).Status);

        // The bank refused the debit, but checkout.session.async_payment_failed never arrived.
        host.Provider.Sessions[sessionId] = new CheckoutSessionSnapshot(
            sessionId, CheckoutSessionState.Complete, false, "pi_refused", null, null,
            PaymentFailed: true, FailureReason: "The account has insufficient funds.");
        await host.ExecuteAsync(
            "UPDATE payments.payment_requests SET updated_at_utc = now() - interval '7 hours' WHERE id = @Id",
            new { Id = paymentRequestId });

        await host.SendAsync(new ReconcilePendingPaymentsCommand(50));

        PaymentRequest failed = await host.GetPaymentRequestAsync(paymentRequestId);
        Assert.Equal(PaymentRequestStatus.Failed, failed.Status);
        Assert.Equal("The account has insufficient funds.", failed.FailureReason);

        await host.RunOutboxAsync();
        Assert.Single(host.EventBus.OfType<PaymentFailedIntegrationEvent>(), e => e.PaymentRequestId == paymentRequestId);
    }

    [Fact]
    public async Task MonthlyCheckout_WithEveryWebhookMissed_IsRestoredByReconciliation()
    {
        // No payer record: this user signed up before Payments existed.
        var payerId = Guid.NewGuid();
        Guid paymentRequestId = await CreateRequestAsync(payerId, PaymentRequestKind.MonthlySubscription);
        await host.SendAsync(new StartCheckoutCommand(paymentRequestId, payerId));
        string sessionId = (await host.GetPaymentRequestAsync(paymentRequestId)).ProviderCheckoutSessionId!;
        string providerSubscriptionId = $"sub_missed_{paymentRequestId:N}";

        host.Provider.Sessions[sessionId] = new CheckoutSessionSnapshot(
            sessionId, CheckoutSessionState.Complete, true, null, "cus_missed", providerSubscriptionId,
            SubscriptionStatus: SubscriptionStatus.Active, SubscriptionPeriodEndUtc: new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc));
        await host.ExecuteAsync(
            """
            UPDATE payments.payment_requests
            SET checkout_expires_at_utc = now() - interval '1 hour', updated_at_utc = now() - interval '1 hour'
            WHERE id = @Id
            """,
            new { Id = paymentRequestId });

        await host.SendAsync(new ReconcilePendingPaymentsCommand(50));

        Assert.Equal(PaymentRequestStatus.Succeeded, (await host.GetPaymentRequestAsync(paymentRequestId)).Status);
        Subscription restored = (await host.GetSubscriptionAsync(paymentRequestId))!;
        Assert.Equal(providerSubscriptionId, restored.ProviderSubscriptionId);
        Assert.Equal(SubscriptionStatus.Active, restored.Status);
        Assert.Equal(new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc), restored.CurrentPeriodEndUtc);

        // Later invoices now find the subscription.
        await PostAndApplyAsync(StripeEvents.Invoice(
            $"evt_missed_invoice_{paymentRequestId:N}", "invoice.paid", $"in_missed_{paymentRequestId:N}", providerSubscriptionId, 1500));
        Assert.Single((await host.GetSubscriptionAsync(paymentRequestId))!.Payments);

        // Without a payer record, the portal opens for the customer behind the subscription.
        Result<CheckoutRedirectResponse> portal = await host.SendAsync(new CreateBillingPortalSessionCommand(payerId));
        Assert.Equal(new Uri("https://billing.stripe.test/cus_missed"), portal.Value.RedirectUrl);

        // A second monthly payment reuses that customer, so one portal covers both.
        Guid second = await CreateRequestAsync(payerId, PaymentRequestKind.MonthlySubscription);
        await host.SendAsync(new StartCheckoutCommand(second, payerId));
        Assert.Equal("cus_missed", Assert.Single(host.Provider.CreatedSessions, s => s.PaymentRequestId == second).ProviderCustomerId);

        await host.RunOutboxAsync();
        Assert.Single(host.EventBus.OfType<SubscriptionActivatedIntegrationEvent>(), e => e.SubscriptionId == paymentRequestId);
    }

    private async Task<Guid> CreateRequestAsync(Guid payerId, PaymentRequestKind kind)
    {
        var paymentRequestId = Guid.NewGuid();
        Result result = await host.SendAsync(new CreatePaymentRequestCommand(
            paymentRequestId,
            "test",
            $"ref-{paymentRequestId:N}",
            payerId,
            kind == PaymentRequestKind.OneOff ? "Gym pass" : "Monthly membership",
            "GBP",
            kind,
            [new PaymentRequestLineDetails("Fee", 1, 1500)]));
        Assert.True(result.IsSuccess);

        return paymentRequestId;
    }

    private async Task PostAndApplyAsync(string payload)
    {
        using HttpResponseMessage response = await host.PostWebhookAsync(payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await host.RunWebhooksAsync();
    }
}
