using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.CreateTeam;

public sealed record CreateTeamCommand(
    string Name,
    AgeGroup AgeGroup,
    string? CoachName = null) : ICommand<Guid>;
