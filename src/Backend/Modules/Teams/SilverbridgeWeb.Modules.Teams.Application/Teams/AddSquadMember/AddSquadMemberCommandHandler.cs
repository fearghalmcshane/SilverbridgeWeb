using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Teams.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.AddSquadMember;

internal sealed class AddSquadMemberCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddSquadMemberCommand>
{
    public async Task<Result> Handle(AddSquadMemberCommand request, CancellationToken cancellationToken)
    {
        Team? team = await teamRepository.GetAsync(request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound(request.TeamId));
        }

        Result result = team.AddSquadMember(request.UserId, request.FirstName, request.LastName, request.JerseyNumber);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
