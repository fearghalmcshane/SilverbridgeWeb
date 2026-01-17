using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;
using SilverbridgeWeb.Modules.Teams.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Teams.Infrastructure.Teams;

internal sealed class TeamRepository(TeamsDbContext context) : ITeamRepository
{
    public async Task<Team?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Teams
            .Include(t => t.SquadMembers)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Team>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Teams
            .Include(t => t.SquadMembers)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Team>> GetByAgeGroupAsync(AgeGroup ageGroup, CancellationToken cancellationToken = default)
    {
        return await context.Teams
            .Include(t => t.SquadMembers)
            .Where(t => t.AgeGroup == ageGroup)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Teams
            .Include(t => t.SquadMembers)
            .SingleOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public void Insert(Team team)
    {
        context.Teams.Add(team);
    }

    public void Update(Team team)
    {
        context.Teams.Update(team);
    }

    public void Delete(Team team)
    {
        context.Teams.Remove(team);
    }
}
