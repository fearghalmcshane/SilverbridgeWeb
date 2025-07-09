using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Categories.GetCategory;

public sealed record GetCategoryQuery(Guid CategoryId) : IQuery<CategoryResponse>;
