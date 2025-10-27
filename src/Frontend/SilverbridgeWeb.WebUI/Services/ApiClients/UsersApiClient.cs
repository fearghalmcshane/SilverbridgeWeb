namespace SilverbridgeWeb.WebUI.Services.ApiClients;

internal sealed class UsersApiClient(HttpClient httpClient) : IApiClient
{
    public string BaseEndpoint => "/users";

    public async Task<LoginResponse?> LoginAsync(string email, string password, bool rememberMe = false)
    {
        var request = new { Email = email, Password = password, RememberMe = rememberMe };

        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/login-cookie", request);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<LoginResponse>()
            : null;
    }

    public async Task<bool> LogoutAsync()
    {
        HttpResponseMessage response = await httpClient.PostAsync($"{BaseEndpoint}/logout", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync()
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{BaseEndpoint}/me");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        }
        catch
        {
            return null;
        }
    }

    public async Task<Guid?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{BaseEndpoint}/register", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }
}

internal sealed record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

internal sealed record RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

internal sealed record LoginResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required IEnumerable<string> Roles { get; init; }
}

internal sealed record RegisterResponse
{
    public required Guid Id { get; init; }
}

internal sealed record CurrentUserResponse
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required IEnumerable<string> Roles { get; init; }
}
