using FluentValidation;

namespace SilverbridgeWeb.Modules.Payments.Application.Payers.UpdatePayer;

internal sealed class UpdatePayerCommandValidator : AbstractValidator<UpdatePayerCommand>
{
    public UpdatePayerCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
    }
}
