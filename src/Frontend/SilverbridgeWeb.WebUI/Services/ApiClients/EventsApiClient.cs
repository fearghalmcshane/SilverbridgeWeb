namespace SilverbridgeWeb.WebUI.Services.ApiClients;

internal sealed class EventsApiClient(HttpClient httpClient) : IApiClient
{
    public string BaseEndpoint => "/events";

    public async Task<IEnumerable<EventResponse>?> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<EventResponse>>(BaseEndpoint, cancellationToken);
    }

    public async Task<EventResponse?> GetEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<EventResponse>($"{BaseEndpoint}/{id}", cancellationToken);
    }

    public async Task<IEnumerable<CategoryResponse>?> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<CategoryResponse>>("/categories", cancellationToken);
    }
}

internal sealed record EventResponse
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    // Add other properties as needed
}

internal sealed record CategoryResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required bool IsArchived { get; init; }
}
