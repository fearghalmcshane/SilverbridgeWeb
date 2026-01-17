namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class SquadMember
{
    private SquadMember()
    {
    }

    public Guid UserId { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public int? JerseyNumber { get; private set; }

    public static SquadMember Create(Guid userId, string firstName, string lastName, int? jerseyNumber = null)
    {
        return new SquadMember
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            JerseyNumber = jerseyNumber
        };
    }

    public void UpdateJerseyNumber(int? jerseyNumber)
    {
        JerseyNumber = jerseyNumber;
    }
}
