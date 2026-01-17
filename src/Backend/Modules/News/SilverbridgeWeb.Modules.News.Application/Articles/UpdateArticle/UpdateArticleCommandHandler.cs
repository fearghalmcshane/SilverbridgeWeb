using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.UpdateArticle;

internal sealed class UpdateArticleCommandHandler(
    IArticleRepository articleRepository,
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

        Result result = article.Update(
            request.Title,
            request.Content,
            request.Excerpt,
            request.FeaturedImage,
            request.CategoryIds);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
