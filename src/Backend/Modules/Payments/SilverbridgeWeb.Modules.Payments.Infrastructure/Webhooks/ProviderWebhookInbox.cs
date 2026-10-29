using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;

internal sealed class ProviderWebhookInbox(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IProviderWebhookInbox
{
    public async Task<bool> AddAsync(
        VerifiedProviderWebhook webhook,
        string payload,
        CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            INSERT INTO payments.provider_webhook_events(id, type, payload, received_at_utc, attempt_count)
            VALUES (@Id, @Type, @Payload, @ReceivedAtUtc, 0)
            ON CONFLICT (id) DO NOTHING
            """;

        int inserted = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = webhook.EventId,
                Type = webhook.EventType,
                Payload = payload,
                ReceivedAtUtc = dateTimeProvider.UtcNow
            },
            cancellationToken: cancellationToken));

        return inserted == 1;
    }
}
