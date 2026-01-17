using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.PublishArticle;

internal sealed class PublishArticleCommandHandler(
    IDateTimeProvider dateTimeProvider,
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<PublishArticleCommand>
{
    public async Task<Result> Handle(PublishArticleCommand request, CancellationToken cancellationToken)
    {
        Article? article = await articleRepository.GetAsync(request.ArticleId, cancellationToken);

        if (article is null)
        {
            return Result.Failure(ArticleErrors.NotFound(request.ArticleId));
        }

        Result result = article.Publish(dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
