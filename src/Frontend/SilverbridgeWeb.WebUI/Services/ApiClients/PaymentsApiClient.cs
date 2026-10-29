using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace SilverbridgeWeb.WebUI.Services.ApiClients;

internal sealed class PaymentsApiClient(HttpClient httpClient) : IApiClient
{
    public string BaseEndpoint => "/payments";

    /// <returns>The payment, or <see langword="null"/> when it does not exist (yet) or belongs to someone else.</returns>
    public async Task<PaymentResponse?> GetPaymentAsync(Guid paymentRequestId, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"{BaseEndpoint}/{paymentRequestId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PaymentResponse>(cancellationToken);
    }

    public async Task<Uri> StartCheckoutAsync(Guid paymentRequestId, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.PostAsync(
            $"{BaseEndpoint}/{paymentRequestId}/checkout",
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        RedirectResponse? redirect = await response.Content.ReadFromJsonAsync<RedirectResponse>(cancellationToken);
        return redirect?.RedirectUrl ?? throw new HttpRequestException("The payment provider did not return a checkout page.");
    }

    public async Task<MyPaymentsResponse?> GetMyPaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<MyPaymentsResponse>($"{BaseEndpoint}/me", cancellationToken);
    }

    public async Task<Uri> OpenBillingPortalAsync(CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.PostAsync(
            $"{BaseEndpoint}/billing-portal",
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        RedirectResponse? redirect = await response.Content.ReadFromJsonAsync<RedirectResponse>(cancellationToken);
        return redirect?.RedirectUrl ?? throw new HttpRequestException("The payment provider did not return a page.");
    }

    public async Task CancelSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.PostAsync(
            $"{BaseEndpoint}/subscriptions/{subscriptionId}/cancel",
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<PagedPaymentsResponse?> GetPaymentsAsync(
        PaymentStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        string query = $"page={page.ToString(CultureInfo.InvariantCulture)}&pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}";
        if (status is not null)
        {
            query += $"&status={status}";
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        return await httpClient.GetFromJsonAsync<PagedPaymentsResponse>($"{BaseEndpoint}?{query}", cancellationToken);
    }

    public async Task RefundAsync(
        Guid paymentRequestId,
        RefundRequest request,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{BaseEndpoint}/{paymentRequestId}/refunds",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetails? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            // Not a problem response; fall back to the status code below.
        }

        string message = problem?.Detail ?? problem?.Title ?? $"Request failed with status code {(int)response.StatusCode}.";
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private sealed record RedirectResponse(Uri RedirectUrl);
}

internal enum PaymentKind
{
    OneOff = 0,
    MonthlySubscription = 1
}

internal enum PaymentStatus
{
    Pending = 0,
    AwaitingPayment = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Expired = 5,
    Cancelled = 6,
    Refunded = 7
}

internal enum RefundStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}

internal enum SubscriptionStatus
{
    Incomplete = 0,
    Active = 1,
    PastDue = 2,
    Cancelled = 3
}

internal sealed record PaymentResponse
{
    public required Guid Id { get; init; }
    public required string Description { get; init; }
    public required PaymentKind Kind { get; init; }
    public required string Currency { get; init; }
    public required long AmountMinor { get; init; }
    public required long AmountRefundedMinor { get; init; }
    public required PaymentStatus Status { get; init; }
    public string? FailureReason { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public IReadOnlyList<PaymentLineResponse> Lines { get; init; } = [];
    public IReadOnlyList<PaymentRefundResponse> Refunds { get; init; } = [];
}

internal sealed record PaymentLineResponse(string Description, int Quantity, long UnitAmountMinor);

internal sealed record PaymentRefundResponse(
    Guid Id,
    long AmountMinor,
    string Reason,
    RefundStatus Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

internal sealed record MyPaymentsResponse(
    IReadOnlyList<PaymentSummaryResponse> Payments,
    IReadOnlyList<SubscriptionSummaryResponse> Subscriptions);

internal sealed record PaymentSummaryResponse(
    Guid Id,
    string Description,
    string Source,
    PaymentKind Kind,
    string Currency,
    long AmountMinor,
    long AmountRefundedMinor,
    PaymentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

internal sealed record SubscriptionSummaryResponse(
    Guid Id,
    string Description,
    string Currency,
    long AmountMinor,
    SubscriptionStatus Status,
    DateTime? CurrentPeriodEndUtc,
    DateTime CreatedAtUtc);

internal sealed record PagedPaymentsResponse(
    IReadOnlyList<AdminPaymentSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

internal sealed record AdminPaymentSummaryResponse(
    Guid Id,
    Guid PayerUserId,
    string? PayerName,
    string? PayerEmail,
    string Description,
    string Source,
    string SourceReference,
    PaymentKind Kind,
    string Currency,
    long AmountMinor,
    long AmountRefundedMinor,
    PaymentStatus Status,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

internal sealed record RefundRequest(Guid RefundId, long? AmountMinor, string Reason);
