using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Articles;

internal sealed class ArticleRepository(NewsDbContext context) : IArticleRepository
{
    public async Task<Article?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Include(a => a.Media)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Include(a => a.Media)
            .SingleOrDefaultAsync(a => a.Slug == slug, cancellationToken);
    }

    public void Insert(Article article)
    {
        context.Articles.Add(article);
    }

    public void AddMedia(ArticleMedia media)
    {
        // ArticleMedia.Id is a client-generated Ulid, not the CLR default (Guid.Empty). When a new
        // ArticleMedia is only added to the tracked Article's Media collection (not via this explicit
        // Add), EF Core's change-detection heuristic can't distinguish it from an existing row whose key
        // is already known, and marks it Modified instead of Added - producing an UPDATE that matches
        // zero rows and throws DbUpdateConcurrencyException. Explicitly adding it here forces the
        // correct Added state.
        context.Set<ArticleMedia>().Add(media);
    }

    public async Task<bool> IsSlugInUseAsync(string slug, Guid? excludeArticleId, CancellationToken cancellationToken = default)
    {
        return await context.Articles.AnyAsync(
            article => article.Slug == slug && (!excludeArticleId.HasValue || article.Id != excludeArticleId.Value),
            cancellationToken);
    }
}
