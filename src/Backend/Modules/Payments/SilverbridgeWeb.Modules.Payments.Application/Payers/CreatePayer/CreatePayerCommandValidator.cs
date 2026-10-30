using FluentValidation;

namespace SilverbridgeWeb.Modules.Payments.Application.Payers.CreatePayer;

internal sealed class CreatePayerCommandValidator : AbstractValidator<CreatePayerCommand>
{
    public CreatePayerCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Email).EmailAddress();
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
    }
}
