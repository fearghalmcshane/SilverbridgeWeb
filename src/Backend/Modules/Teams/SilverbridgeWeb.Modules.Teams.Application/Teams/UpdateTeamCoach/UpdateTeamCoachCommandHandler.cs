using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Teams.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.UpdateTeamCoach;

internal sealed class UpdateTeamCoachCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTeamCoachCommand>
{
    public async Task<Result> Handle(UpdateTeamCoachCommand request, CancellationToken cancellationToken)
    {
        Team? team = await teamRepository.GetAsync(request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound(request.TeamId));
        }

        team.UpdateCoach(request.CoachName);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
