using System.Collections.Concurrent;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.IntegrationTests;

/// <summary>Stands in for Stripe's API; webhooks still go through the real verifier and parser.</summary>
internal sealed class FakePaymentProvider : IPaymentProvider
{
    private int _sessionCount;
    private int _refundCount;

    public ConcurrentBag<ProviderCustomerRequest> CreatedCustomers { get; } = [];

    public ConcurrentBag<CheckoutSessionRequest> CreatedSessions { get; } = [];

    public ConcurrentBag<string> ExpiredSessions { get; } = [];

    public ConcurrentBag<ProviderRefundRequest> CreatedRefunds { get; } = [];

    public ConcurrentBag<string> CancelledSubscriptions { get; } = [];

    public ConcurrentDictionary<string, CheckoutSessionSnapshot> Sessions { get; } = new();

    public Task<Result<string>> CreateCustomerAsync(ProviderCustomerRequest request, CancellationToken cancellationToken = default)
    {
        CreatedCustomers.Add(request);
        return Task.FromResult(Result.Success($"cus_{request.UserId:N}"));
    }

    public Task<Result<CheckoutSession>> CreateCheckoutSessionAsync(
        CheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        CreatedSessions.Add(request);
        string sessionId = $"cs_test_{Interlocked.Increment(ref _sessionCount)}";
        Sessions[sessionId] = new CheckoutSessionSnapshot(
            sessionId, CheckoutSessionState.Open, false, null, request.ProviderCustomerId, null);

        return Task.FromResult(Result.Success(
            new CheckoutSession(sessionId, new Uri($"https://checkout.stripe.test/{sessionId}"), request.ExpiresAtUtc)));
    }

    public Task<Result<CheckoutSessionSnapshot>> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Sessions.TryGetValue(sessionId, out CheckoutSessionSnapshot? snapshot)
            ? Result.Success(snapshot)
            : Result.Failure<CheckoutSessionSnapshot>(ProviderErrors.RequestFailed("read the checkout session")));
    }

    public Task<Result> ExpireCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ExpiredSessions.Add(sessionId);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<ProviderRefund>> CreateRefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        CreatedRefunds.Add(request);
        string refundId = $"re_test_{Interlocked.Increment(ref _refundCount)}";
        return Task.FromResult(Result.Success(new ProviderRefund(refundId, RefundStatus.Succeeded, null)));
    }

    public Task<Result> CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        CancelledSubscriptions.Add(providerSubscriptionId);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<Uri>> CreateBillingPortalSessionAsync(
        string providerCustomerId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success(new Uri($"https://billing.stripe.test/{providerCustomerId}")));
    }
}

internal sealed class RecordingEventBus : IEventBus
{
    public ConcurrentQueue<IIntegrationEvent> Published { get; } = new();

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        Published.Enqueue(integrationEvent);
        return Task.CompletedTask;
    }

    public IEnumerable<T> OfType<T>() => Published.OfType<T>();
}
