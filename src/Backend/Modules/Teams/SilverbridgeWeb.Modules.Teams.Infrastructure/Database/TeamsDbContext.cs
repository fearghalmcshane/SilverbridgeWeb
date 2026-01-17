using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SilverbridgeWeb.Common.Infrastructure.Inbox;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.Teams.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;
using SilverbridgeWeb.Modules.Teams.Infrastructure.Teams;

namespace SilverbridgeWeb.Modules.Teams.Infrastructure.Database;

public sealed class TeamsDbContext(DbContextOptions<TeamsDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Team> Teams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Teams);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
    }
}
