using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeam;

internal sealed class GetTeamQueryHandler(ITeamRepository teamRepository)
    : IQueryHandler<GetTeamQuery, TeamDetailResponse>
{
    public async Task<Result<TeamDetailResponse>> Handle(
        GetTeamQuery request,
        CancellationToken cancellationToken)
    {
        Team? team = await teamRepository.GetAsync(request.Id, cancellationToken);

        if (team is null)
        {
            return Result.Failure<TeamDetailResponse>(TeamErrors.NotFound(request.Id));
        }

        TeamDetailResponse response = new(
            team.Id,
            team.Name,
            team.AgeGroup,
            team.SportType,
            team.CoachName,
            team.IsActive,
            team.SquadMembers.Select(m => new SquadMemberResponse(
                m.UserId,
                m.FirstName,
                m.LastName,
                m.JerseyNumber)).ToList());

        return response;
    }
}
