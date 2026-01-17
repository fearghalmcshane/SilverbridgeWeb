using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SilverbridgeWeb.Common.Infrastructure.Inbox;
using SilverbridgeWeb.Common.Infrastructure.Outbox;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;
using SilverbridgeWeb.Modules.News.Infrastructure.Articles;
using SilverbridgeWeb.Modules.News.Infrastructure.Categories;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Database;

public sealed class NewsDbContext(DbContextOptions<NewsDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Article> Articles { get; set; }

    internal DbSet<Category> Categories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.News);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new ArticleConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
    }
}
