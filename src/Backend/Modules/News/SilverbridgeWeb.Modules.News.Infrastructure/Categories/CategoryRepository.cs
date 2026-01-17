using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.News.Domain.Categories;
using SilverbridgeWeb.Modules.News.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.News.Infrastructure.Categories;

internal sealed class CategoryRepository(NewsDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Categories.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public void Insert(Category category)
    {
        context.Categories.Add(category);
    }

    public void Update(Category category)
    {
        context.Categories.Update(category);
    }
}
