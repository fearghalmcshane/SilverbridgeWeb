using FluentValidation;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;

internal sealed class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Slug)
            .NotEmpty()
            .MaximumLength(200)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(c => c.Summary).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.Content).NotEmpty().MaximumLength(50000);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.AuthorUserId).NotEmpty();
        RuleFor(c => c.AuthorFirstName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.AuthorLastName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ArticleType).IsInEnum();

        When(c => c.ArticleType == ArticleType.MatchReport, () =>
        {
            RuleFor(c => c.HomeTeam).NotEmpty().MaximumLength(200);
            RuleFor(c => c.AwayTeam).NotEmpty().MaximumLength(200);
            RuleFor(c => c.HomeGoals).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(c => c.HomePoints).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(c => c.AwayGoals).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(c => c.AwayPoints).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(c => c.Competition).MaximumLength(200);
            RuleFor(c => c.Venue).MaximumLength(200);
        });
    }
}
