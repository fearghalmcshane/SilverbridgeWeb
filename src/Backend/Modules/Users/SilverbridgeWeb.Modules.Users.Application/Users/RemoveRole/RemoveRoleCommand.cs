using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.RemoveRole;

public sealed record RemoveRoleCommand(Guid UserId, string RoleName) : ICommand;
