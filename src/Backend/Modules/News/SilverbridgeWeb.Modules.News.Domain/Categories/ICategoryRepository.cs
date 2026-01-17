namespace SilverbridgeWeb.Modules.News.Domain.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    void Insert(Category category);

    void Update(Category category);
}
