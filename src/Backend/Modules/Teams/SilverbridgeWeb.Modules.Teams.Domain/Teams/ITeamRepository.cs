namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public interface ITeamRepository
{
    Task<Team?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Team>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Team>> GetByAgeGroupAsync(AgeGroup ageGroup, CancellationToken cancellationToken = default);

    Task<Team?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    void Insert(Team team);

    void Update(Team team);

    void Delete(Team team);
}
