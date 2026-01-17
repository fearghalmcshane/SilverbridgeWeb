using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Teams.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.RemoveSquadMember;

internal sealed class RemoveSquadMemberCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveSquadMemberCommand>
{
    public async Task<Result> Handle(RemoveSquadMemberCommand request, CancellationToken cancellationToken)
    {
        Team? team = await teamRepository.GetAsync(request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound(request.TeamId));
        }

        Result result = team.RemoveSquadMember(request.UserId);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
