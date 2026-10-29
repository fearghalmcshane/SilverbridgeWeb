using Microsoft.AspNetCore.Http;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Infrastructure.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Authentication;

internal sealed class PaymentsUserContext(IHttpContextAccessor httpContextAccessor) : IPaymentsUserContext
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                          throw new SilverbridgeWebException("User identifier is unavailable");

    public bool HasPermission(string permission) =>
        httpContextAccessor.HttpContext?.User is { } user && user.GetPermissions().Contains(permission);
}
