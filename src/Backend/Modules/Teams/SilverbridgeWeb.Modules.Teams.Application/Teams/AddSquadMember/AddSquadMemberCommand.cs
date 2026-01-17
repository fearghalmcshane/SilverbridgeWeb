using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.AddSquadMember;

public sealed record AddSquadMemberCommand(
    Guid TeamId,
    Guid UserId,
    string FirstName,
    string LastName,
    int? JerseyNumber = null) : ICommand;
