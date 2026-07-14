using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Bookings.Domain.Facilities;

public sealed class Facility : Entity
{
    private Facility()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string Color { get; private set; }

    public static Facility Create(string name, string description, string color)
    {
        return new Facility
        {
            Id = Ulid.NewUlid().ToGuid(),
            Name = name,
            Description = description,
            Color = color
        };
    }
}
