using System.Net.Http.Json;

namespace SilverbridgeWeb.WebUI.Services.ApiClients;

internal sealed class UsersApiClient(HttpClient httpClient) : IApiClient
{
    public string BaseEndpoint => "/users";

    public async Task<UserProfileResponse?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<UserProfileResponse>($"{BaseEndpoint}/profile", cancellationToken);
    }

    public async Task<UserRolesResponse?> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<UserRolesResponse>($"{BaseEndpoint}/roles", cancellationToken);
    }

    public async Task AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{BaseEndpoint}/{userId}/roles",
            new AssignRoleRequest(roleName),
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync(
            $"{BaseEndpoint}/{userId}/roles/{roleName}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

internal sealed record UserProfileResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

internal sealed record UserRolesResponse
{
    public required Guid UserId { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
}

internal sealed record AssignRoleRequest(string RoleName);
