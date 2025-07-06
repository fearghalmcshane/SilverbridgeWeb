namespace SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;

public  interface IUnitOfwork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
