using System.Data.Common;
using Npgsql;
using SilverbridgeWeb.Common.Application.Data;

namespace SilverbridgeWeb.Common.Infrastructure.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await dataSource.OpenConnectionAsync();
    }
}
