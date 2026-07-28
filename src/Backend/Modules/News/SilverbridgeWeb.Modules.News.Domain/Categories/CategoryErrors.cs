using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Categories;

public static class CategoryErrors
{
    public static Error NotFound(Guid categoryId) =>
        Error.NotFound("NewsCategories.NotFound", $"The news category with the identifier {categoryId} was not found");

    public static readonly Error AlreadyArchived = Error.Problem(
        "NewsCategories.AlreadyArchived",
        "The news category was already archived");
}
