using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Application.Users.SyncUserFromClerk;

namespace SilverbridgeWeb.Migrator;

internal sealed class ClerkUserBackfillService(
    HttpClient httpClient,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    ILogger<ClerkUserBackfillService> logger)
{
    private const int PageSize = 100;

    public async Task<ClerkUserBackfillSummary> BackfillAsync(CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Clerk backfill starting. Environment: {EnvironmentName}", hostEnvironment.EnvironmentName);
        }

        bool isEnabled = configuration.GetValue<bool?>("Clerk:BackfillOnStartup") ?? false;
        if (!isEnabled)
        {
            logger.LogInformation("Skipping Clerk user backfill because Clerk:BackfillOnStartup is disabled.");
            return ClerkUserBackfillSummary.SkipResult;
        }

        string apiKey = configuration["Clerk:ApiKey"]
            ?? throw new InvalidOperationException("Clerk:ApiKey is not configured for user backfill.");

        string apiBaseUrl = configuration["Clerk:ApiBaseUrl"] ?? "https://api.clerk.com/v1/";
        httpClient.BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute);

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        int fetched = 0;
        int synced = 0;
        int skipped = 0;
        int failed = 0;

        int offset = 0;

        while (true)
        {
            List<ClerkUserDto> users = await FetchUsersPageAsync(apiKey, offset, cancellationToken);

            if (users.Count == 0)
            {
                break;
            }

            fetched += users.Count;

            foreach (ClerkUserDto user in users)
            {
                string? email = ResolvePrimaryEmail(user);
                if (string.IsNullOrWhiteSpace(email))
                {
                    skipped++;
                    logger.LogWarning("Skipping Clerk user {ClerkUserId}: no primary email.", user.Id);
                    continue;
                }

                string firstName = string.IsNullOrWhiteSpace(user.FirstName) ? "Unknown" : user.FirstName;
                string lastName = string.IsNullOrWhiteSpace(user.LastName) ? "User" : user.LastName;

                Result<Guid> result = await sender.Send(
                    new SyncUserFromClerkCommand(user.Id, email, firstName, lastName),
                    cancellationToken);

                if (result.IsFailure)
                {
                    failed++;
                    logger.LogWarning(
                        "Failed syncing Clerk user {ClerkUserId}: {Code} - {Description}",
                        user.Id,
                        result.Error.Code,
                        result.Error.Description);
                    continue;
                }

                synced++;
            }

            if (users.Count < PageSize)
            {
                break;
            }

            offset += users.Count;
        }

        return new ClerkUserBackfillSummary(fetched, synced, skipped, failed, false);
    }

    private async Task<List<ClerkUserDto>> FetchUsersPageAsync(string apiKey, int offset, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"users?limit={PageSize}&offset={offset}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException("Clerk API key is invalid or lacks permissions for listing users.");
        }

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Failed to fetch Clerk users. HTTP {(int)response.StatusCode}: {body}");
        }

        List<ClerkUserDto>? users = await response.Content.ReadFromJsonAsync<List<ClerkUserDto>>(cancellationToken);

        return users ?? [];
    }

    private static string? ResolvePrimaryEmail(ClerkUserDto user)
    {
        if (!string.IsNullOrWhiteSpace(user.PrimaryEmailAddressId))
        {
            string? primary = user.EmailAddresses
                .FirstOrDefault(e => e.Id == user.PrimaryEmailAddressId)
                ?.EmailAddress;

            if (!string.IsNullOrWhiteSpace(primary))
            {
                return primary;
            }
        }

        return user.EmailAddresses
            .Select(e => e.EmailAddress)
            .FirstOrDefault(email => !string.IsNullOrWhiteSpace(email));
    }

    private sealed class ClerkUserDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("first_name")]
        public string FirstName { get; init; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; init; } = string.Empty;

        [JsonPropertyName("primary_email_address_id")]
        public string PrimaryEmailAddressId { get; init; } = string.Empty;

        [JsonPropertyName("email_addresses")]
        public List<ClerkEmailDto> EmailAddresses { get; init; } = [];
    }

    private sealed class ClerkEmailDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("email_address")]
        public string EmailAddress { get; init; } = string.Empty;
    }
}

internal sealed record ClerkUserBackfillSummary(int Fetched, int Synced, int Skipped, int Failed, bool IsSkipped)
{
    public static ClerkUserBackfillSummary SkipResult => new(0, 0, 0, 0, true);
}
