using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, string RoleName) : ICommand;
