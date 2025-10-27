using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Users.Application.Abstractions.Identity;

public interface IIdentityProviderService
{
    string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
}
