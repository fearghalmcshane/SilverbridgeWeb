using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Categories.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyCollection<CategoryResponse>>;
