namespace SilverbridgeWeb.Modules.News.Application.Categories.GetCategories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive);
