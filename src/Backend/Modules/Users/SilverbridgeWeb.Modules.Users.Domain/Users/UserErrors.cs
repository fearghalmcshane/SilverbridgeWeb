using Microsoft.AspNetCore.Identity;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Users.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("Users.NotFound", $"The user with the identifier {userId} not found");

    public static Error NotFound(string identityId) =>
        Error.NotFound("Users.NotFound", $"The user with the IDP identifier {identityId} not found");

    public static Error UserRegistrationFailed(string result) =>
        Error.Failure("Users.RegistrationFailed", $"The user registration failed with error: {result}");

    public static Error LoginFailed() =>
        Error.Failure("Users.LoginFailed", "The user login failed, Invalid email or password");
}
