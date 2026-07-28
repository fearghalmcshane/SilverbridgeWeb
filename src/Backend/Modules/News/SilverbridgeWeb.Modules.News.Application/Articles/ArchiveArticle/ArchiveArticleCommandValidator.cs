using FluentValidation;

namespace SilverbridgeWeb.Modules.News.Application.Articles.ArchiveArticle;

internal sealed class ArchiveArticleCommandValidator : AbstractValidator<ArchiveArticleCommand>
{
    public ArchiveArticleCommandValidator()
    {
        RuleFor(c => c.ArticleId).NotEmpty();
    }
}
