using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Teams.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Teams.Domain.Teams;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.CreateTeam;

internal sealed class CreateTeamCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTeamCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        Team? existingTeam = await teamRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existingTeam is not null)
        {
            return Result.Failure<Guid>(TeamErrors.NameRequired);
        }

        Result<Team> result = Team.Create(
            request.Name,
            request.AgeGroup,
            request.SportType,
            request.CoachName);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        teamRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
