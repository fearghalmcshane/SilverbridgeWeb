using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Users.Domain.Users;

namespace SilverbridgeWeb.Modules.Users.Application.Users.SyncUserFromClerk;

internal sealed class SyncUserFromClerkCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<SyncUserFromClerkCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SyncUserFromClerkCommand request, CancellationToken cancellationToken)
    {
        User? existingUser = await userRepository.GetByIdentityIdAsync(request.ClerkUserId, cancellationToken);

        if (existingUser is not null)
        {
            existingUser.Update(request.FirstName, request.LastName);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existingUser.Id;
        }

        var user = User.Create(request.Email, request.FirstName, request.LastName, request.ClerkUserId);

        userRepository.Insert(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
