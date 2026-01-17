using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Categories;

public sealed class Category : Entity
{
    private Category()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static Category Create(string name, string? description = null)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true
        };

        category.Raise(new CategoryCreatedDomainEvent(category.Id, category.Name));

        return category;
    }

    public void Update(string name, string? description = null)
    {
        if (Name == name && Description == description)
        {
            return;
        }

        Name = name;
        Description = description;

        Raise(new CategoryUpdatedDomainEvent(Id, Name));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        Raise(new CategoryDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;

        Raise(new CategoryActivatedDomainEvent(Id));
    }
}
