using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<UserRolesResponse>;

public sealed record UserRolesResponse(Guid UserId, IReadOnlyList<string> Roles);
