using MediatR;
using SilverbridgeWeb.Common.Application.Authorization;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Application.Users.GetUserPermissions;

namespace SilverbridgeWeb.Modules.Users.Infrastructure.Authorization;

internal sealed class PermissionService(ISender sender) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        return await sender.Send(new GetUserPermissionsQuery(identityId));
    }
}
