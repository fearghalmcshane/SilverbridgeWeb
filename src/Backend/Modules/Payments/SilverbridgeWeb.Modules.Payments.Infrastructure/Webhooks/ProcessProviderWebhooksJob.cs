using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Application.Webhooks.ApplyProviderEvent;
using SilverbridgeWeb.Modules.Payments.Infrastructure.StripeIntegration;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;

/// <summary>
/// Applies stored provider callbacks. An event that refers to something not recorded yet (events can arrive out
/// of order) is retried with a growing delay; an event that can never apply is set aside with its error.
/// </summary>
[DisallowConcurrentExecution]
internal sealed class ProcessProviderWebhooksJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<PaymentsOptions> options,
    ILogger<ProcessProviderWebhooksJob> logger) : IJob
{
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromHours(1);

    public async Task Execute(IJobExecutionContext context)
    {
        WebhookProcessingOptions webhookOptions = options.Value.Webhooks;

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(context.CancellationToken);

        IReadOnlyList<PendingWebhook> webhooks = await GetPendingAsync(connection, transaction, webhookOptions);

        foreach (PendingWebhook webhook in webhooks)
        {
            Outcome outcome;
            try
            {
                outcome = await ApplyAsync(webhook, context.CancellationToken);
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Payments - Exception while applying provider webhook {EventId}", webhook.Id);
                outcome = Outcome.Retry(exception.ToString());
            }

            await RecordOutcomeAsync(connection, transaction, webhook, outcome, webhookOptions.MaxAttempts);
        }

        await transaction.CommitAsync(context.CancellationToken);
    }

    private async Task<Outcome> ApplyAsync(PendingWebhook webhook, CancellationToken cancellationToken)
    {
        ProviderEvent? providerEvent = StripeWebhookEventParser.Parse(webhook.Payload);
        if (providerEvent is null)
        {
            return Outcome.Done;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        Result result = await sender.Send(new ApplyProviderEventCommand(providerEvent), cancellationToken);
        if (result.IsSuccess)
        {
            return Outcome.Done;
        }

        string error = $"{result.Error.Code}: {result.Error.Description}";
        if (result.Error.Type is ErrorType.NotFound or ErrorType.Failure)
        {
            return Outcome.Retry(error);
        }

        logger.LogError(
            "Payments - Provider webhook {EventId} ({EventType}) cannot be applied: {Error}",
            webhook.Id,
            webhook.Type,
            error);

        return Outcome.Rejected(error);
    }

    private async Task<IReadOnlyList<PendingWebhook>> GetPendingAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        WebhookProcessingOptions webhookOptions)
    {
        const string sql =
            $"""
             SELECT
                 id AS {nameof(PendingWebhook.Id)},
                 type AS {nameof(PendingWebhook.Type)},
                 payload AS {nameof(PendingWebhook.Payload)},
                 attempt_count AS {nameof(PendingWebhook.AttemptCount)}
             FROM payments.provider_webhook_events
             WHERE processed_at_utc IS NULL
               AND attempt_count < @MaxAttempts
               AND (next_attempt_at_utc IS NULL OR next_attempt_at_utc <= @UtcNow)
             ORDER BY received_at_utc
             LIMIT @BatchSize
             FOR UPDATE SKIP LOCKED
             """;

        IEnumerable<PendingWebhook> webhooks = await connection.QueryAsync<PendingWebhook>(
            sql,
            new { webhookOptions.MaxAttempts, webhookOptions.BatchSize, dateTimeProvider.UtcNow },
            transaction: transaction);

        return webhooks.AsList();
    }

    private async Task RecordOutcomeAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        PendingWebhook webhook,
        Outcome outcome,
        int maxAttempts)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;
        int attemptCount = webhook.AttemptCount + 1;

        if (outcome.ShouldRetry && attemptCount >= maxAttempts)
        {
            logger.LogError(
                "Payments - Gave up on provider webhook {EventId} ({EventType}) after {Attempts} attempts: {Error}",
                webhook.Id,
                webhook.Type,
                attemptCount,
                outcome.Error);
        }

        const string sql =
            """
            UPDATE payments.provider_webhook_events
            SET attempt_count = @AttemptCount,
                processed_at_utc = @ProcessedAtUtc,
                next_attempt_at_utc = @NextAttemptAtUtc,
                error = @Error
            WHERE id = @Id
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                webhook.Id,
                AttemptCount = attemptCount,
                ProcessedAtUtc = outcome.ShouldRetry ? (DateTime?)null : utcNow,
                NextAttemptAtUtc = outcome.ShouldRetry ? utcNow + RetryDelay(attemptCount) : (DateTime?)null,
                outcome.Error
            },
            transaction: transaction);
    }

    internal static TimeSpan RetryDelay(int attemptCount)
    {
        double seconds = 15 * Math.Pow(2, Math.Min(attemptCount - 1, 10));

        return TimeSpan.FromSeconds(Math.Min(seconds, MaxRetryDelay.TotalSeconds));
    }

    internal sealed record PendingWebhook(string Id, string Type, string Payload, int AttemptCount);

    private sealed record Outcome(bool ShouldRetry, string? Error)
    {
        public static readonly Outcome Done = new(false, null);

        public static Outcome Retry(string error) => new(true, error);

        public static Outcome Rejected(string error) => new(false, error);
    }
}
