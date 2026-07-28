using FluentValidation;

namespace SilverbridgeWeb.Modules.News.Application.Articles.AddArticleMedia;

internal sealed class AddArticleMediaCommandValidator : AbstractValidator<AddArticleMediaCommand>
{
    public AddArticleMediaCommandValidator()
    {
        RuleFor(c => c.ArticleId).NotEmpty();
        RuleFor(c => c.BlobUrl).NotEmpty().MaximumLength(2048);
        RuleFor(c => c.MediaType).NotEmpty().Must(t => t is "image" or "video");
        RuleFor(c => c.AltText).MaximumLength(500);
        RuleFor(c => c.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
