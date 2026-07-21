using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Users.Domain.Users;

namespace SilverbridgeWeb.Modules.Users.Application.Users.AssignRole;

internal sealed class AssignRoleCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<AssignRoleCommand>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        Role? role = await roleRepository.GetAsync(request.RoleName, cancellationToken);
        if (role is null)
        {
            return Result.Failure(UserErrors.RoleNotFound(request.RoleName));
        }

        user.AssignRole(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
