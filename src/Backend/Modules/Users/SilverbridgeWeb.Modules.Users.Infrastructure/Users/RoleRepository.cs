using Microsoft.EntityFrameworkCore;
using SilverbridgeWeb.Modules.Users.Domain.Users;
using SilverbridgeWeb.Modules.Users.Infrastructure.Database;

namespace SilverbridgeWeb.Modules.Users.Infrastructure.Users;

internal sealed class RoleRepository(UsersDbContext context) : IRoleRepository
{
    public async Task<Role?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Roles.SingleOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles.ToListAsync(cancellationToken);
    }
}
