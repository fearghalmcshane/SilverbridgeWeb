using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Quartz;
using SilverbridgeWeb.Common.Application;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Infrastructure.Eventbus;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Matches.IntegrationEvents;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;
using SilverbridgeWeb.Modules.News.Infrastructure;
using SilverbridgeWeb.Modules.News.Infrastructure.Database;
using SilverbridgeWeb.Modules.News.Infrastructure.Inbox;
using Testcontainers.PostgreSql;

namespace SilverbridgeWeb.Modules.News.IntegrationTests;

public sealed class MatchCompletedInboxPostgreSqlTests
{
    [Fact]
    public async Task MatchCompleted_CreatesOneDraftMatchReportPerTeamTallyMatch()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await using PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await postgres.StartAsync(cancellationToken);
        string connectionString = postgres.GetConnectionString();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:silverbridgeDb"] = connectionString,
                ["News:Inbox:IntervalInSeconds"] = "60",
                ["News:Inbox:BatchSize"] = "20",
                ["News:Outbox:IntervalInSeconds"] = "60",
                ["News:Outbox:BatchSize"] = "20"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<InsertOutboxMessagesInterceptor>();
        services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();
        services.AddSingleton<IDbConnectionFactory>(new TestDbConnectionFactory(connectionString));
        services.AddApplication([SilverbridgeWeb.Modules.News.Application.AssemblyReference.Assembly]);
        services.AddNewsModule(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider();

        await using (AsyncServiceScope migrationScope = provider.CreateAsyncScope())
        {
            await migrationScope.ServiceProvider.GetRequiredService<NewsDbContext>()
                .Database.MigrateAsync(cancellationToken);
        }

        var inboxJob = new ProcessInboxJob(
            provider.GetRequiredService<IDbConnectionFactory>(),
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<IDateTimeProvider>(),
            Options.Create(new InboxOptions { BatchSize = 20 }),
            provider.GetRequiredService<ILogger<ProcessInboxJob>>());
        IJobExecutionContext jobContext = DispatchProxy.Create<IJobExecutionContext, TestJobExecutionContext>();
        ((TestJobExecutionContext)(object)jobContext).CancellationToken = cancellationToken;

        MatchCompletedIntegrationEvent first = CreateEvent("match/opaque", "away");
        await ConsumeAsync(provider, first, cancellationToken);
        await inboxJob.Execute(jobContext);

        // A later event for the same Team Tally match (e.g. a replayed webhook) must not duplicate the report.
        await ConsumeAsync(provider, CreateEvent("match/opaque", "home"), cancellationToken);
        await inboxJob.Execute(jobContext);

        // A different match between the same teams on the same day gets its own report with a unique slug.
        await ConsumeAsync(provider, CreateEvent("match/replay", "home"), cancellationToken);
        await inboxJob.Execute(jobContext);

        Assert.Empty(await QueryInboxErrorsAsync(connectionString, cancellationToken));

        await using AsyncServiceScope verifyScope = provider.CreateAsyncScope();
        NewsDbContext database = verifyScope.ServiceProvider.GetRequiredService<NewsDbContext>();
        List<Article> articles = await database.Set<Article>()
            .AsNoTracking()
            .Include(article => article.MatchReportDetails)
            .OrderBy(article => article.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        Assert.Equal(2, articles.Count);

        Article report = articles.Single(article => article.MatchReportDetails!.SourceMatchId == "match/opaque");
        Assert.Equal(ArticleStatus.Draft, report.Status);
        Assert.Null(report.PublishedAtUtc);
        Assert.Equal(ArticleType.MatchReport, report.ArticleType);
        Assert.Equal(WellKnownCategories.MatchReportsId, report.CategoryId);
        Assert.Equal(SystemAuthors.TeamTally.UserId, report.AuthorUserId);
        Assert.Equal("Team Tally", $"{report.AuthorFirstName} {report.AuthorLastName}");
        Assert.Equal("Cúil Óg beat Áth na Rí in Senior Championship Final", report.Title);
        Assert.Equal("cuil-og-v-ath-na-ri-2026-10-11", report.Slug);

        MatchReportDetails details = report.MatchReportDetails!;
        Assert.Equal("Áth na Rí", details.HomeTeam);
        Assert.Equal("Cúil Óg", details.AwayTeam);
        Assert.Equal((1, 8, 2, 10), (details.HomeGoals, details.HomePoints, details.AwayGoals, details.AwayPoints));
        Assert.Equal("Senior Championship", details.Competition);
        Assert.Equal("Páirc Uí Mhuirí", details.Venue);
        Assert.Equal(new DateTime(2026, 10, 11, 19, 0, 0, DateTimeKind.Utc), details.MatchDateUtc);
        Assert.Equal(MatchReportSources.TeamTally, details.SourceProvider);

        Article replay = articles.Single(article => article.MatchReportDetails!.SourceMatchId == "match/replay");
        Assert.Equal("cuil-og-v-ath-na-ri-2026-10-11-2", replay.Slug);
        Assert.Equal("Cúil Óg", replay.MatchReportDetails!.HomeTeam);
    }

    private static async Task ConsumeAsync(
        IServiceProvider provider,
        MatchCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IIntegrationEventConsumer<MatchCompletedIntegrationEvent> consumer = Assert.Single(
            scope.ServiceProvider.GetServices<IIntegrationEventConsumer<MatchCompletedIntegrationEvent>>());
        await consumer.ConsumeAsync(integrationEvent, cancellationToken);
    }

    private static async Task<IReadOnlyList<string>> QueryInboxErrorsAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        IEnumerable<string> errors = await connection.QueryAsync<string>(
            "SELECT COALESCE(error, 'unprocessed') FROM news.inbox_messages WHERE processed_on_utc IS NULL OR error IS NOT NULL");
        return errors.AsList();
    }

    private static MatchCompletedIntegrationEvent CreateEvent(string matchId, string venueType)
    {
        var endedAtUtc = new DateTime(2026, 10, 11, 19, 0, 0, DateTimeKind.Utc);
        var snapshot = new MatchCompletedSnapshot(
            $"evt-{Guid.NewGuid()}",
            matchId,
            "match.completed",
            "2026-10-01",
            endedAtUtc.AddMinutes(5),
            endedAtUtc.AddMinutes(6),
            "completed",
            "hurling",
            "Senior Championship",
            "Final",
            endedAtUtc,
            "Páirc Uí Mhuirí",
            venueType,
            "Cúil Óg",
            new MatchScore(2, 10, 16, "2-10"),
            "Áth na Rí",
            new MatchScore(1, 8, 11, "1-08"),
            "win",
            "Won",
            null);

        return new MatchCompletedIntegrationEvent(Guid.NewGuid(), endedAtUtc.AddMinutes(6), Guid.NewGuid(), snapshot);
    }

    private sealed class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class TestDbConnectionFactory(string connectionString) : IDbConnectionFactory
    {
        public async ValueTask<DbConnection> OpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            return connection;
        }
    }

    [SuppressMessage("Design", "CA1515", Justification = "DispatchProxy emits a derived proxy type in a separate assembly.")]
    public class TestJobExecutionContext : DispatchProxy
    {
        public CancellationToken CancellationToken { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == "get_CancellationToken")
            {
                return CancellationToken;
            }

            throw new NotSupportedException($"Unexpected Quartz job context member: {targetMethod?.Name}");
        }
    }
}
