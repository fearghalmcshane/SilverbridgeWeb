using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Articles;

internal sealed class ArticleRepository(NewsDbContext context) : IArticleRepository
{
    public async Task<Article?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Article>> GetPublishedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Where(a => a.Status == ArticleStatus.Published)
            .OrderByDescending(a => a.PublishedAtUtc)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Article>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Article>> GetByCategoryAsync(
        Guid categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Query using the join table via raw SQL for the article IDs, then load articles
        string sql = $"""
            SELECT DISTINCT a.id
            FROM news.articles a
            INNER JOIN news.article_categories ac ON a.id = ac.article_id
            WHERE a.status = {(int)ArticleStatus.Published}
              AND ac.category_id = {categoryId}
            ORDER BY a.published_at_utc DESC
            LIMIT {pageSize} OFFSET {page * pageSize}
            """;

        List<Guid> articleIds = await context.Database
            .SqlQueryRaw<Guid>(sql, categoryId)
            .ToListAsync(cancellationToken);

        if (!articleIds.Any())
        {
            return Array.Empty<Article>();
        }

        return await context.Articles
            .Where(a => articleIds.Contains(a.Id))
            .OrderByDescending(a => a.PublishedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Insert(Article article)
    {
        context.Articles.Add(article);
    }

    public void Update(Article article)
    {
        context.Articles.Update(article);
    }
}
