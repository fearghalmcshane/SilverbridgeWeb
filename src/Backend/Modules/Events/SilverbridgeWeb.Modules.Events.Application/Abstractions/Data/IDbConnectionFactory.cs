using System.Data.Common;

namespace SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
