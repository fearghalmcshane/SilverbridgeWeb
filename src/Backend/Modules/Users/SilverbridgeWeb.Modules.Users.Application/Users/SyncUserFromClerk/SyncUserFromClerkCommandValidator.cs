using FluentValidation;

namespace SilverbridgeWeb.Modules.Users.Application.Users.SyncUserFromClerk;

internal sealed class SyncUserFromClerkCommandValidator : AbstractValidator<SyncUserFromClerkCommand>
{
    public SyncUserFromClerkCommandValidator()
    {
        RuleFor(c => c.ClerkUserId).NotEmpty();
        RuleFor(c => c.Email).EmailAddress();
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
    }
}
