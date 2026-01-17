using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeam;

public sealed record SquadMemberResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    int? JerseyNumber);

public sealed record TeamDetailResponse(
    Guid Id,
    string Name,
    AgeGroup AgeGroup,
    string? CoachName,
    bool IsActive,
    IReadOnlyCollection<SquadMemberResponse> SquadMembers);
