using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string FirstName, string LastName) : ICommand;
