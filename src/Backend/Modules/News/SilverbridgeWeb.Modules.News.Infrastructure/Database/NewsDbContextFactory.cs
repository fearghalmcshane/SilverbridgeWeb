using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Database;

internal sealed class NewsDbContextFactory : IDesignTimeDbContextFactory<NewsDbContext>
{
    public NewsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NewsDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=silverbridgeweb;Username=postgres",
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.News))
            .UseSnakeCaseNamingConvention();

        return new NewsDbContext(optionsBuilder.Options);
    }
}
