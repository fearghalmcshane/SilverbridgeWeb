using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Users.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("Users.NotFound", $"The user with the identifier {userId} not found");

    public static Error NotFound(string identityId) =>
        Error.NotFound("Users.NotFound", $"The user with the IDP identifier {identityId} not found");

    public static Error RoleNotFound(string roleName) =>
        Error.NotFound("Users.RoleNotFound", $"The role '{roleName}' does not exist");

    public static Error RoleNotAssigned(string roleName) =>
        Error.Problem("Users.RoleNotAssigned", $"The role '{roleName}' is not assigned to this user");
}
