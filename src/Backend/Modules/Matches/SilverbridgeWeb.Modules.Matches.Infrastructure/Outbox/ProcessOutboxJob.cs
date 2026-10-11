using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Infrastructure.Serialization;
using SilverbridgeWeb.Common.Infrastructure.Outbox;

namespace SilverbridgeWeb.Modules.Matches.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<MatchesOutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger) : IJob
{
    private const int MaxAttempts = 5;

    public async Task Execute(IJobExecutionContext context)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(context.CancellationToken);
        IEnumerable<OutboxMessageResponse> queryResult = await connection.QueryAsync<OutboxMessageResponse>(
            """
            SELECT id AS Id, content AS Content, attempt_count AS AttemptCount
            FROM matches.outbox_messages
            WHERE processed_on_utc IS NULL
              AND exhausted_on_utc IS NULL
              AND (next_attempt_on_utc IS NULL OR next_attempt_on_utc <= @Now)
            ORDER BY occurred_on_utc
            LIMIT @BatchSize
            FOR UPDATE SKIP LOCKED
            """,
            new { Now = dateTimeProvider.UtcNow, outboxOptions.Value.BatchSize },
            transaction).ConfigureAwait(false);
        IReadOnlyList<OutboxMessageResponse> messages = queryResult.ToList();

        foreach (OutboxMessageResponse message in messages)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    message.Content,
                    SerializerSettings.Instance)!;
                using IServiceScope scope = serviceScopeFactory.CreateScope();
                IEnumerable<IDomainEventHandler> handlers = DomainEventHandlersFactory.GetHandlers(
                    domainEvent.GetType(),
                    scope.ServiceProvider,
                    Application.AssemblyReference.Assembly);

                foreach (IDomainEventHandler handler in handlers)
                {
                    await handler.Handle(domainEvent, context.CancellationToken);
                }

                await connection.ExecuteAsync(
                    """
                    UPDATE matches.outbox_messages
                    SET processed_on_utc = @ProcessedOnUtc,
                        error = NULL,
                        next_attempt_on_utc = NULL
                    WHERE id = @Id
                    """,
                    new { message.Id, ProcessedOnUtc = dateTimeProvider.UtcNow },
                    transaction);
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (
                context.CancellationToken.IsCancellationRequested &&
                ContainsCancellation(exception))
            {
                throw new OperationCanceledException(context.CancellationToken);
            }
            catch (Exception exception)
            {
                int attempt = message.AttemptCount + 1;
                bool exhausted = attempt >= MaxAttempts;
                DateTime? nextAttempt = exhausted ? null : dateTimeProvider.UtcNow.AddSeconds(10 * Math.Pow(2, attempt - 1));
                await connection.ExecuteAsync(
                    """
                    UPDATE matches.outbox_messages
                    SET attempt_count = @AttemptCount,
                        next_attempt_on_utc = @NextAttemptOnUtc,
                        exhausted_on_utc = @ExhaustedOnUtc,
                        error = @Error
                    WHERE id = @Id
                    """,
                    new
                    {
                        message.Id,
                        AttemptCount = attempt,
                        NextAttemptOnUtc = nextAttempt,
                        ExhaustedOnUtc = exhausted ? dateTimeProvider.UtcNow : (DateTime?)null,
                        Error = exception.ToString()
                    },
                    transaction);

                if (exhausted)
                {
                    logger.LogError(
                        exception,
                        "Matches outbox message {MessageId} exhausted all {MaxAttempts} publication attempts.",
                        message.Id,
                        MaxAttempts);
                }
                else
                {
                    logger.LogWarning(
                        exception,
                        "Matches outbox message {MessageId} failed attempt {AttemptCount}; retry scheduled for {NextAttemptOnUtc}.",
                        message.Id,
                        attempt,
                        nextAttempt);
                }
            }
        }

        await transaction.CommitAsync(context.CancellationToken);
    }

    private static bool ContainsCancellation(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            return true;
        }

        return exception is AggregateException aggregate
            ? aggregate.InnerExceptions.Any(ContainsCancellation)
            : exception.InnerException is not null && ContainsCancellation(exception.InnerException);
    }

    internal sealed record OutboxMessageResponse(Guid Id, string Content, int AttemptCount);
}
