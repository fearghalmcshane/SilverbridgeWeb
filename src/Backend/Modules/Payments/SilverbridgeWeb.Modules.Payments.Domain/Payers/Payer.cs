using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Payments.Domain.Payers;

/// <summary>A user as Payments knows them, kept in step with the Users module through integration events.</summary>
public sealed class Payer : Entity
{
    private Payer()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string? ProviderCustomerId { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public static Payer Create(Guid userId, string email, string firstName, string lastName)
    {
        return new Payer
        {
            Id = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void AssignProviderCustomer(string providerCustomerId)
    {
        ProviderCustomerId = providerCustomerId;
    }
}
