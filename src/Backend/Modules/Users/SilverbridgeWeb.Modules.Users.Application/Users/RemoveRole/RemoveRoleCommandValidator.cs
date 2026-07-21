using FluentValidation;

namespace SilverbridgeWeb.Modules.Users.Application.Users.RemoveRole;

internal sealed class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.RoleName).NotEmpty();
    }
}
