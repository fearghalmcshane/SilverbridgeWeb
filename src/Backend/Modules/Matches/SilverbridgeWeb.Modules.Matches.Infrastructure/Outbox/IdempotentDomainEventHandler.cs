using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Matches.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory dbConnectionFactory)
    : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        var marker = new OutboxConsumerMarker(domainEvent.Id, decorated.GetType().Name);
        bool exists = await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
                SELECT 1 FROM matches.outbox_message_consumers
                WHERE outbox_message_id = @OutboxMessageId AND name = @Name)
            """,
            marker);
        if (exists)
        {
            return;
        }

        await decorated.Handle(domainEvent, cancellationToken);
        await connection.ExecuteAsync(
            """
            INSERT INTO matches.outbox_message_consumers(outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            """,
            marker);
    }

    private sealed record OutboxConsumerMarker(Guid OutboxMessageId, string Name);
}
