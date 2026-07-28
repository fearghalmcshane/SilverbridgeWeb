namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public interface IArticleRepository
{
    Task<Article?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    void Insert(Article article);

    void AddMedia(ArticleMedia media);

    Task<bool> IsSlugInUseAsync(string slug, Guid? excludeArticleId, CancellationToken cancellationToken = default);
}
