using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.Application.Articles.UpdateArticle;

internal sealed class UpdateArticleCommandHandler(
    IDateTimeProvider dateTimeProvider,
    IArticleRepository articleRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateArticleCommand>
{
    public async Task<Result> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        Article? article = await articleRepository.GetAsync(request.ArticleId, cancellationToken);

        if (article is null)
        {
            return Result.Failure(ArticleErrors.NotFound(request.ArticleId));
        }

        Category? category = await categoryRepository.GetAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure(ArticleErrors.CategoryNotFound(request.CategoryId));
        }

        if (await articleRepository.IsSlugInUseAsync(request.Slug, request.ArticleId, cancellationToken))
        {
            return Result.Failure(ArticleErrors.SlugAlreadyInUse);
        }

        article.Update(
            request.Title,
            request.Slug,
            request.Summary,
            request.Content,
            request.CategoryId,
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
