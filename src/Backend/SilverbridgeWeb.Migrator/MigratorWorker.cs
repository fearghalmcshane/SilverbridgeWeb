using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Attendance.Infrastructure.Database;
using SilverbridgeWeb.Modules.Bookings.Infrastructure.Database;
using SilverbridgeWeb.Modules.Events.Infrastructure.Database;
using SilverbridgeWeb.Modules.Ticketing.Infrastructure.Database;
using SilverbridgeWeb.Modules.Users.Infrastructure.Database;

namespace SilverbridgeWeb.Migrator;

internal sealed partial class MigratorWorker(
    IServiceProvider serviceProvider,
    ClerkUserBackfillService clerkUserBackfillService,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<MigratorWorker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using Activity? activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            await MigrateAsync<EventsDbContext>(stoppingToken);
            await MigrateAsync<UsersDbContext>(stoppingToken);
            await MigrateAsync<TicketingDbContext>(stoppingToken);
            await MigrateAsync<AttendanceDbContext>(stoppingToken);
            await MigrateAsync<BookingsDbContext>(stoppingToken);

            ClerkUserBackfillSummary summary = await clerkUserBackfillService.BackfillAsync(stoppingToken);
            if (summary.IsSkipped)
            {
                LogClerkBackfillSkipped();
            }
            else
            {
                LogClerkBackfillCompleted(summary.Fetched, summary.Synced, summary.Skipped, summary.Failed);
            }
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateAsync<TDbContext>(CancellationToken cancellationToken)
        where TDbContext : DbContext
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            string contextName = typeof(TDbContext).Name;
            LogMigrating(contextName);
            await dbContext.Database.MigrateAsync(cancellationToken);
            LogMigrationComplete(contextName);
        });
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrating {DbContext}...")]
    private partial void LogMigrating(string dbContext);

    [LoggerMessage(Level = LogLevel.Information, Message = "Migration complete for {DbContext}.")]
    private partial void LogMigrationComplete(string dbContext);

    [LoggerMessage(Level = LogLevel.Information, Message = "Clerk user backfill skipped.")]
    private partial void LogClerkBackfillSkipped();

    [LoggerMessage(Level = LogLevel.Information, Message = "Clerk user backfill complete. Fetched: {Fetched}, Synced: {Synced}, Skipped: {Skipped}, Failed: {Failed}")]
    private partial void LogClerkBackfillCompleted(int fetched, int synced, int skipped, int failed);
}
