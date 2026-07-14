using System.Net.Http.Json;

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
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task<IEnumerable<BookingItemResponse>?> GetBookingsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<BookingItemResponse>>(BaseEndpoint, cancellationToken);
    }

    public async Task<Guid?> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(BaseEndpoint, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
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
    public required DateTime StartsAtUtc { get; init; }
    public required DateTime EndsAtUtc { get; init; }
    public required bool IsPublic { get; init; }
    public required string Status { get; init; }
}

internal sealed record AddFacilityRequest(string Name, string Description, string Color);

internal sealed record CreateBookingRequest(
    Guid FacilityId,
    string Title,
    string BookerName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsPublic);
