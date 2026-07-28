using FluentValidation;

namespace SilverbridgeWeb.Modules.News.Application.Articles.PublishArticle;

internal sealed class PublishArticleCommandValidator : AbstractValidator<PublishArticleCommand>
{
    public PublishArticleCommandValidator()
    {
        RuleFor(c => c.ArticleId).NotEmpty();
    }
}
