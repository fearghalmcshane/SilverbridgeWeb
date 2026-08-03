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
            .Include(a => a.MatchReportDetails)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Include(a => a.Media)
            .Include(a => a.MatchReportDetails)
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

    public void TrackMatchReportDetails(MatchReportDetails details)
    {
        // MatchReportDetails uses ArticleId as a shared primary key, so unlike ArticleMedia it doesn't
        // have a client-generated key that could be misread by EF's Added/Modified heuristics. However,
        // it's still attached to an already-tracked Article via a reference navigation rather than an
        // explicit DbSet.Add(), so a brand-new instance is Detached until we explicitly add it here.
        // Existing (already-tracked) instances are left alone - their in-place mutation is already
        // picked up by EF's normal change detection.
        if (context.Entry(details).State == EntityState.Detached)
        {
            context.Set<MatchReportDetails>().Add(details);
        }
    }

    public void RemoveMatchReportDetails(MatchReportDetails details)
    {
        context.Set<MatchReportDetails>().Remove(details);
    }

    public async Task<bool> IsSlugInUseAsync(string slug, Guid? excludeArticleId, CancellationToken cancellationToken = default)
    {
        return await context.Articles.AnyAsync(
            article => article.Slug == slug && (!excludeArticleId.HasValue || article.Id != excludeArticleId.Value),
            cancellationToken);
    }
}
