using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Users.Application.Users.SyncUserFromClerk;

public sealed record SyncUserFromClerkCommand(string ClerkUserId, string Email, string FirstName, string LastName)
    : ICommand<Guid>;
