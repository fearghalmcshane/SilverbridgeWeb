using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Categories;

public sealed class CategoryCreatedDomainEvent(Guid CategoryId, string Name) : DomainEvent
{
    public Guid CategoryId { get; } = CategoryId;
    public string Name { get; } = Name;
}
