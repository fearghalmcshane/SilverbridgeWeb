using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<string>;
