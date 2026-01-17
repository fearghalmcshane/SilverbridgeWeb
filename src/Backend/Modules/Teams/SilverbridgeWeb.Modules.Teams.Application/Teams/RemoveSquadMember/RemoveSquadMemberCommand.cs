using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.RemoveSquadMember;

public sealed record RemoveSquadMemberCommand(
    Guid TeamId,
    Guid UserId) : ICommand;
