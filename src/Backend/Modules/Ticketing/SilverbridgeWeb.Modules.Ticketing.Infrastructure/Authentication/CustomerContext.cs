using Microsoft.AspNetCore.Http;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Infrastructure.Authentication;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Authentication;

namespace SilverbridgeWeb.Modules.Ticketing.Infrastructure.Authentication;

internal sealed class CustomerContext(IHttpContextAccessor httpContextAccessor) : ICustomerContext
{
    public Guid CustomerId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                              throw new SilverbridgeWebException("User identifier is unavailable");
}
