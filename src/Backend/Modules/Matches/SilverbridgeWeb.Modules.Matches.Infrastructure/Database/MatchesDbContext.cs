using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SilverbridgeWeb.Common.Infrastructure.Inbox;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Matches.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;

namespace SilverbridgeWeb.Modules.Matches.Infrastructure.Database;

public sealed class MatchesDbContext(DbContextOptions<MatchesDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<MatchCompletion> MatchCompletions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Matches);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new MatchCompletionConfiguration());
        modelBuilder.Entity<OutboxMessage>()
            .Property<int>("AttemptCount")
            .HasDefaultValue(0)
            .IsRequired();
        modelBuilder.Entity<OutboxMessage>()
            .Property<DateTime?>("NextAttemptOnUtc");
        modelBuilder.Entity<OutboxMessage>()
            .Property<DateTime?>("ExhaustedOnUtc");
    }
}
