using SilverbridgeWeb.Common.Application.Data;

namespace SilverbridgeWeb.Modules.Teams.Application.Abstractions.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
