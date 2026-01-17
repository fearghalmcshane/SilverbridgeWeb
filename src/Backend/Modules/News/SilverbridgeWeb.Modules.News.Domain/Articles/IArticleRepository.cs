namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public interface IArticleRepository
{
    Task<Article?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Article>> GetPublishedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Article>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Article>> GetByCategoryAsync(
        Guid categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Insert(Article article);

    void Update(Article article);
}
