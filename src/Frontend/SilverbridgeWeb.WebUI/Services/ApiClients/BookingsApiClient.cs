using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace SilverbridgeWeb.WebUI.Services.ApiClients;

internal sealed class BookingsApiClient(HttpClient httpClient) : IApiClient
{
    public string BaseEndpoint => "/bookings";

    public async Task<IEnumerable<FacilityBookingResponse>?> GetFacilitiesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<FacilityBookingResponse>>($"{BaseEndpoint}/facilities", cancellationToken);
    }

    public async Task<Guid?> AddFacilityAsync(AddFacilityRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/facilities", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task<IEnumerable<BookingItemResponse>?> GetBookingsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        string from = Uri.EscapeDataString(fromUtc.ToString("O"));
        string to = Uri.EscapeDataString(toUtc.ToString("O"));

        return await httpClient.GetFromJsonAsync<IEnumerable<BookingItemResponse>>(
            $"{BaseEndpoint}?fromUtc={from}&toUtc={to}",
            cancellationToken);
    }

    public async Task<Guid?> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseEndpoint, request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task ApproveBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PutAsync(
            $"{BaseEndpoint}/{bookingId}/approve",
            content: null,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateBookingAsync(
        Guid bookingId,
        UpdateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync(
            $"{BaseEndpoint}/{bookingId}",
            request,
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync(
            $"{BaseEndpoint}/{bookingId}",
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

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        string message = problem?.Detail ?? problem?.Title ?? $"Request failed with status code {(int)response.StatusCode}.";
        throw new HttpRequestException(message, null, response.StatusCode);
    }
}

internal sealed record FacilityBookingResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
}

internal sealed record BookingItemResponse
{
    public required Guid Id { get; init; }
    public required Guid FacilityId { get; init; }
    public required string Title { get; init; }
    public required string BookerName { get; init; }
    public required string ContactName { get; init; }
    public required DateTime StartsAtUtc { get; init; }
    public required DateTime EndsAtUtc { get; init; }
    public required bool IsPublic { get; init; }
    public required BookingStatus Status { get; init; }
}

internal enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}

internal sealed record AddFacilityRequest(string Name, string Description, string Color);

internal sealed record CreateBookingRequest(
    Guid FacilityId,
    string Title,
    string ContactName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsPublic,
    bool IsRecurring,
    IReadOnlyCollection<DayOfWeek> RecurrenceDays,
    DateTime? RecurrenceEndDate);

internal sealed record UpdateBookingRequest(
    Guid FacilityId,
    string Title,
    string ContactName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsPublic);
