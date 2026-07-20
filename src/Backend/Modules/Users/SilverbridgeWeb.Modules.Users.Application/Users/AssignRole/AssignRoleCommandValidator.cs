using FluentValidation;

namespace SilverbridgeWeb.Modules.Users.Application.Users.AssignRole;

internal sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.RoleName).NotEmpty();
    }
}
