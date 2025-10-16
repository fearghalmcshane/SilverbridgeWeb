using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
    public static readonly Error EmailIsNotUnique = Error.Conflict(
        "Identity.EmailIsNotUnique",
        "The provided email is already in use."
        );
}
