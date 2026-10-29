using FluentValidation;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.GetPayments;

internal sealed class GetPaymentsQueryValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsQueryValidator()
    {
        RuleFor(q => q.Status).IsInEnum().When(q => q.Status is not null);
        RuleFor(q => q.Search).MaximumLength(100);
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
    }
}
