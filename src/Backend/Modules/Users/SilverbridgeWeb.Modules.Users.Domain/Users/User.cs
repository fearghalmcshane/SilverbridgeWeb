using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Users.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = [];

    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string IdentityId { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    public static User Create(string email, string firstName, string lastName, string identityId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IdentityId = identityId
        };

        user._roles.Add(Role.Member);

        user.Raise(new UserRegisteredDomainEvent(user.Id));

        return user;
    }

    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Name == role.Name))
        {
            return;
        }

        _roles.Add(role);
    }

    public Result RemoveRole(Role role)
    {
        Role? existing = _roles.FirstOrDefault(r => r.Name == role.Name);

        if (existing is null)
        {
            return Result.Failure(UserErrors.RoleNotAssigned(role.Name));
        }

        _roles.Remove(existing);
        return Result.Success();
    }

    public void Update(string firstName, string lastName)
    {
        if (FirstName == firstName && LastName == lastName)
        {
            return;
        }

        FirstName = firstName;
        LastName = lastName;

        Raise(new UserProfileUpdatedDomainEvent(Id, FirstName, LastName));
    }
}
