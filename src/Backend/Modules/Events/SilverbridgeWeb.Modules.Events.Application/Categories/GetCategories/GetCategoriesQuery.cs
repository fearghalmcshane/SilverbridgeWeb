using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Events.Application.Categories.GetCategory;

namespace SilverbridgeWeb.Modules.Events.Application.Categories.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyCollection<CategoryResponse>>;
