using System.Globalization;
using System.Text.Json;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;

namespace SilverbridgeWeb.Modules.Matches.Presentation;

public sealed class TeamTallyWebhookPayloadParser
{
    public static ParsedTeamTallyEvent Parse(string originalJson, DateTime receivedAtUtc)
    {
        using var document = JsonDocument.Parse(originalJson);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new TeamTallyPayloadException("Webhook envelope must be a JSON object.");
        }

        string eventId = RequiredString(root, "id");
        string eventType = RequiredString(root, "type");
        string apiVersion = RequiredString(root, "api_version");
        DateTime createdAt = RequiredTimestamp(root, "created_at");

        if (!eventType.Equals("match.completed", StringComparison.Ordinal))
        {
            return new ParsedTeamTallyEvent(eventId, eventType, apiVersion, createdAt, null);
        }

        JsonElement data = RequiredObject(root, "data");
        string matchId = RequiredString(data, "id");
        string status = RequiredString(data, "status");
        if (!status.Equals("completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new TeamTallyPayloadException("Completed match data must have completed status.");
        }

        JsonElement team = RequiredObject(data, "team");
        JsonElement opponent = RequiredObject(data, "opponent");
        JsonElement venue = OptionalObject(data, "venue");

        var snapshot = new MatchCompletionSnapshot(
            eventId,
            matchId,
            eventType,
            apiVersion,
            createdAt,
            DateTime.SpecifyKind(receivedAtUtc, DateTimeKind.Utc),
            status,
            RequiredString(data, "sport"),
            OptionalString(data, "competition"),
            OptionalString(data, "stage"),
            RequiredTimestamp(data, "ended_at"),
            venue.ValueKind == JsonValueKind.Object ? OptionalString(venue, "name") : null,
            venue.ValueKind == JsonValueKind.Object ? OptionalString(venue, "type") : null,
            RequiredString(team, "name"),
            ParseScore(RequiredObject(team, "score"), "team.score"),
            RequiredString(opponent, "name"),
            ParseScore(RequiredObject(opponent, "score"), "opponent.score"),
            RequiredString(data, "result"),
            OptionalString(data, "summary"),
            OptionalUri(data, "live_url"));

        return new ParsedTeamTallyEvent(eventId, eventType, apiVersion, createdAt, snapshot);
    }

    private static MatchScoreSnapshot ParseScore(JsonElement score, string path) =>
        new(
            RequiredNonnegativeInteger(score, "goals", path),
            RequiredNonnegativeInteger(score, "points", path),
            RequiredNonnegativeInteger(score, "total", path),
            RequiredString(score, "display"));

    private static int RequiredNonnegativeInteger(JsonElement element, string name, string path)
    {
        if (!element.TryGetProperty(name, out JsonElement value) ||
            value.ValueKind != JsonValueKind.Number ||
            !value.TryGetInt32(out int number) ||
            number < 0)
        {
            throw new TeamTallyPayloadException($"'{path}.{name}' must be a nonnegative integer.");
        }

        return number;
    }

    private static string RequiredString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) ||
            value.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new TeamTallyPayloadException($"'{name}' is required and must be a non-empty string.");
        }

        return value.GetString()!;
    }

    private static string? OptionalString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new TeamTallyPayloadException($"'{name}' must be a string when supplied.");
        }

        return value.GetString();
    }

    private static Uri? OptionalUri(JsonElement element, string name)
    {
        string? value = OptionalString(element, name);
        if (value is null)
        {
            return null;
        }

        if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri? uri))
        {
            throw new TeamTallyPayloadException($"'{name}' must be a valid URL when supplied.");
        }

        return uri;
    }

    private static DateTime RequiredTimestamp(JsonElement element, string name)
    {
        string timestamp = RequiredString(element, name);
        if (!DateTimeOffset.TryParse(
                timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTimeOffset parsed))
        {
            throw new TeamTallyPayloadException($"'{name}' must be a valid timestamp.");
        }

        return parsed.UtcDateTime;
    }

    private static JsonElement RequiredObject(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.Object)
        {
            throw new TeamTallyPayloadException($"'{name}' must be an object.");
        }

        return value;
    }

    private static JsonElement OptionalObject(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
        {
            return default;
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new TeamTallyPayloadException($"'{name}' must be an object when supplied.");
        }

        return value;
    }
}

public sealed record ParsedTeamTallyEvent(
    string EventId,
    string EventType,
    string ApiVersion,
    DateTime CreatedAtUtc,
    MatchCompletionSnapshot? Completion);

public sealed class TeamTallyPayloadException(string message) : Exception(message)
{
}
