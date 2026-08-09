namespace SilverbridgeWeb.Common.Application.Authorization;

public sealed record PermissionsResponse(Guid UserId, string DisplayName, HashSet<string> Permissions);
