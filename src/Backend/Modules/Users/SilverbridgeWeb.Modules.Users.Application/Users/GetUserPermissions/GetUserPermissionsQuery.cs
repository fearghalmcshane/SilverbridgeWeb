using SilverbridgeWeb.Common.Application.Authorization;
using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
