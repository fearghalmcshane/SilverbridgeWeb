using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.UpdateTeamCoach;

public sealed record UpdateTeamCoachCommand(
    Guid TeamId,
    string? CoachName) : ICommand;
