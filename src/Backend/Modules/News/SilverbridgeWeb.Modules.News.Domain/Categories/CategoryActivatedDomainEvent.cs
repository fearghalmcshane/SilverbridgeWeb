using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Categories;

public sealed class CategoryActivatedDomainEvent(Guid CategoryId) : DomainEvent
{
    public Guid CategoryId { get; } = CategoryId;
}
