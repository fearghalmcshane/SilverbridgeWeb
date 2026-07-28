using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;

internal sealed class CreateArticleCommandHandler(
    IDateTimeProvider dateTimeProvider,
    ICategoryRepository categoryRepository,
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateArticleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        Category? category = await categoryRepository.GetAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure<Guid>(ArticleErrors.CategoryNotFound(request.CategoryId));
        }

        if (await articleRepository.IsSlugInUseAsync(request.Slug, null, cancellationToken))
        {
            return Result.Failure<Guid>(ArticleErrors.SlugAlreadyInUse);
        }

        Result<Article> result = Article.Create(
            category,
            request.AuthorUserId,
            request.AuthorFirstName,
            request.AuthorLastName,
            request.Title,
            request.Slug,
            request.Summary,
            request.Content,
            dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        articleRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
