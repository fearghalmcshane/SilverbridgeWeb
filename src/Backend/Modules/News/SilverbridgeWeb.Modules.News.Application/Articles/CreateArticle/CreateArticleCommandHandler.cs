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
            request.ArticleType,
            dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        Article article = result.Value;

        if (request.ArticleType == ArticleType.MatchReport)
        {
            article.SetMatchReportDetails(
                request.HomeTeam!,
                request.AwayTeam!,
                request.HomeGoals!.Value,
                request.HomePoints!.Value,
                request.AwayGoals!.Value,
                request.AwayPoints!.Value,
                request.Competition,
                request.Venue,
                request.MatchDateUtc);
        }

        articleRepository.Insert(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return article.Id;
    }
}
