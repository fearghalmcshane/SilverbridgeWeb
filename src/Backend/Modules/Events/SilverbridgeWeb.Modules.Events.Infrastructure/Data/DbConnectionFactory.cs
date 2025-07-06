using System.Data.Common;
using Npgsql;
using SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;

namespace SilverbridgeWeb.Modules.Events.Infrastructure.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await dataSource.OpenConnectionAsync();
    }
}
