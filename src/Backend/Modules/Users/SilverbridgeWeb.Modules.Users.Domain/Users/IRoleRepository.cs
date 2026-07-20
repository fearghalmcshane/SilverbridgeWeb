namespace SilverbridgeWeb.Modules.Users.Domain.Users;

public interface IRoleRepository
{
    Task<Role?> GetAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}
