using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Categories;

public sealed class CategoryDeactivatedDomainEvent(Guid CategoryId) : DomainEvent
{
    public Guid CategoryId { get; } = CategoryId;
}
