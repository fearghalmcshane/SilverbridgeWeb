using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
