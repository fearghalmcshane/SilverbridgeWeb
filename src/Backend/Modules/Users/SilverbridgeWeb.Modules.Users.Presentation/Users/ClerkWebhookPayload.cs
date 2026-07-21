using System.Text.Json.Serialization;

namespace SilverbridgeWeb.Modules.Users.Presentation.Users;

internal sealed class ClerkWebhookPayload
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public ClerkUserData Data { get; init; } = new();
}

internal sealed class ClerkUserData
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("email_addresses")]
    public List<ClerkEmailAddress> EmailAddresses { get; init; } = [];

    [JsonPropertyName("primary_email_address_id")]
    public string? PrimaryEmailAddressId { get; init; }

    public string PrimaryEmail =>
        EmailAddresses.FirstOrDefault(e => e.Id == PrimaryEmailAddressId)?.EmailAddress
        ?? EmailAddresses.FirstOrDefault()?.EmailAddress
        ?? string.Empty;
}

internal sealed class ClerkEmailAddress
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("email_address")]
    public string EmailAddress { get; init; } = string.Empty;
}
