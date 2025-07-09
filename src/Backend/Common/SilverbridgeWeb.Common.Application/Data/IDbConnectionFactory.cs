using System.Data.Common;

namespace SilverbridgeWeb.Common.Application.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
