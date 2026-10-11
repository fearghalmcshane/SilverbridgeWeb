using Microsoft.EntityFrameworkCore;
using Npgsql;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;
using SilverbridgeWeb.Modules.Matches.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Matches.Infrastructure.Matches;

internal sealed class MatchCompletionRepository(MatchesDbContext dbContext) : IMatchCompletionRepository
{
    internal const string ProviderMatchUniqueConstraint = "ux_match_completions_provider_match_id";
    internal const string ProviderEventUniqueConstraint = "ux_match_completions_provider_event_id";

    public async Task<MatchCompletionCaptureOutcome> CaptureAsync(
        MatchCompletion completion,
        CancellationToken cancellationToken)
    {
        MatchCompletion? existingMatch = await dbContext.MatchCompletions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.ProviderMatchId == completion.ProviderMatchId,
                cancellationToken);
        if (existingMatch is not null)
        {
            return MatchCompletionCaptureOutcome.AlreadyCaptured;
        }

        MatchCompletion? existingEvent = await dbContext.MatchCompletions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.ProviderEventId == completion.ProviderEventId,
                cancellationToken);
        if (existingEvent is not null)
        {
            return MatchCompletionCaptureOutcome.EventIdConflict;
        }

        dbContext.MatchCompletions.Add(completion);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return MatchCompletionCaptureOutcome.Captured;
        }
        catch (DbUpdateException exception) when (IsRelevantUniqueViolation(exception))
        {
            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in dbContext.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.State = EntityState.Detached;
                }
            }

            dbContext.ChangeTracker.Clear();
            MatchCompletion? winner = await dbContext.MatchCompletions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.ProviderMatchId == completion.ProviderMatchId,
                    cancellationToken);
            if (winner is not null)
            {
                return MatchCompletionCaptureOutcome.AlreadyCaptured;
            }

            MatchCompletion? eventOwner = await dbContext.MatchCompletions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.ProviderEventId == completion.ProviderEventId,
                    cancellationToken);
            if (eventOwner is not null && eventOwner.ProviderMatchId != completion.ProviderMatchId)
            {
                return MatchCompletionCaptureOutcome.EventIdConflict;
            }

            throw;
        }
    }

    private static bool IsRelevantUniqueViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: ProviderMatchUniqueConstraint or ProviderEventUniqueConstraint
                })
            {
                return true;
            }
        }

        return false;
    }
}
