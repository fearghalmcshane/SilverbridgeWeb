using Microsoft.AspNetCore.Http;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Infrastructure.Authentication;
using SilverbridgeWeb.Modules.Attendance.Application.Abstractions.Authentication;

namespace SilverbridgeWeb.Modules.Attendance.Infrastructure.Authentication;

internal sealed class AttendanceContext(IHttpContextAccessor httpContextAccessor) : IAttendanceContext
{
    public Guid AttendeeId => httpContextAccessor.HttpContext?.User.GetUserId() ??
                              throw new SilverbridgeWebException("User identifier is unavailable");
}
