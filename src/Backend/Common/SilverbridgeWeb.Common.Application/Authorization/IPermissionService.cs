using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Common.Application.Authorization;

public interface IPermissionService
{
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}
