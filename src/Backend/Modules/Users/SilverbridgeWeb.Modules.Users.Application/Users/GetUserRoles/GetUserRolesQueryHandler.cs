using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Domain.Users;

namespace SilverbridgeWeb.Modules.Users.Application.Users.GetUserRoles;

internal sealed class GetUserRolesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetUserRolesQuery, UserRolesResponse>
{
    public async Task<Result<UserRolesResponse>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT ur.role_name AS {nameof(UserRole.RoleName)}
             FROM users.users u
             JOIN users.user_roles ur ON ur.user_id = u.id
             WHERE u.id = @UserId
             """;

        List<UserRole> roles = (await connection.QueryAsync<UserRole>(sql, request)).AsList();

        if (!roles.Any())
        {
            return Result.Failure<UserRolesResponse>(UserErrors.NotFound(request.UserId));
        }

        return new UserRolesResponse(request.UserId, roles.Select(r => r.RoleName).ToList());
    }

    internal sealed class UserRole
    {
        internal string RoleName { get; init; } = string.Empty;
    }
}
