using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeams;

public sealed record TeamResponse(
    Guid Id,
    string Name,
    AgeGroup AgeGroup,
    SportType SportType,
    string? CoachName,
    bool IsActive,
    int SquadMemberCount);
