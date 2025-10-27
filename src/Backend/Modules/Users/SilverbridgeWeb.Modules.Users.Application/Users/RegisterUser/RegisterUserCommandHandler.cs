using Microsoft.AspNetCore.Identity;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Identity;
using SilverbridgeWeb.Modules.Users.Domain.Users;

namespace SilverbridgeWeb.Modules.Users.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler(UserManager<User> userManager)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(request.Email, request.FirstName, request.LastName);

        IdentityResult result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Result.Failure<Guid>(UserErrors.UserRegistrationFailed(result.ToString()));
        }

        return user.Id;
    }
}

internal sealed record RegisterResponse
{
    public required Guid Id { get; init; }
}
