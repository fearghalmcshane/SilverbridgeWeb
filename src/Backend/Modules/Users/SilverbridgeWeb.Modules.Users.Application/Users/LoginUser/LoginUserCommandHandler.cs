using Microsoft.AspNetCore.Identity;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Identity;
using SilverbridgeWeb.Modules.Users.Domain.Users;

namespace SilverbridgeWeb.Modules.Users.Application.Users.LoginUser;

internal sealed class LoginUserCommandHandler(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IIdentityProviderService identityProviderService)
    : ICommandHandler<LoginUserCommand, string>
{
    public async Task<Result<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFound(request.Email));
        }

        SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Result.Failure<string>(UserErrors.LoginFailed());
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        string token = identityProviderService.GenerateToken(user.Id, user.Email!, roles);

        return token;
    }
}
