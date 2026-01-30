using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public sealed class Team : Entity
{
    private readonly List<SquadMember> _squadMembers = [];

    private Team()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public SportType SportType { get; private set; }

    public AgeGroup AgeGroup { get; private set; }

    public string? CoachName { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<SquadMember> SquadMembers => _squadMembers.AsReadOnly();

    public static Result<Team> Create(
        string name,
        AgeGroup ageGroup,
        SportType sportType,
        string? coachName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Team>(TeamErrors.NameRequired);
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name,
            AgeGroup = ageGroup,
            SportType = sportType,
            CoachName = coachName,
            IsActive = true
        };

        team.Raise(new TeamCreatedDomainEvent(team.Id, team.Name, team.AgeGroup, team.SportType));

        return team;
    }

    public Result AddSquadMember(Guid userId, string firstName, string lastName, int? jerseyNumber = null)
    {
        if (_squadMembers.Any(m => m.UserId == userId))
        {
            return Result.Failure(TeamErrors.SquadMemberAlreadyExists);
        }

        var squadMember = SquadMember.Create(userId, firstName, lastName, jerseyNumber);
        _squadMembers.Add(squadMember);

        Raise(new SquadMemberAddedDomainEvent(Id, squadMember.UserId, squadMember.FirstName, squadMember.LastName));

        return Result.Success();
    }

    public Result RemoveSquadMember(Guid userId)
    {
        SquadMember? member = _squadMembers.FirstOrDefault(m => m.UserId == userId);
        if (member is null)
        {
            return Result.Failure(TeamErrors.SquadMemberNotFound);
        }

        _squadMembers.Remove(member);

        Raise(new SquadMemberRemovedDomainEvent(Id, userId));

        return Result.Success();
    }

    public void UpdateCoach(string? coachName)
    {
        if (CoachName == coachName)
        {
            return;
        }

        CoachName = coachName;

        Raise(new TeamCoachUpdatedDomainEvent(Id, CoachName));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        Raise(new TeamDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;

        Raise(new TeamActivatedDomainEvent(Id));
    }

    public void ChangeName(string name)
    {
        if (Name == name)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        Name = name;

        Raise(new TeamNameChangedDomainEvent(Id, Name));
    }
}
