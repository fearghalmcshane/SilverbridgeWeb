using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.ArchiveArticle;

internal sealed class ArchiveArticleCommandHandler(IArticleRepository articleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ArchiveArticleCommand>
{
    public async Task<Result> Handle(ArchiveArticleCommand request, CancellationToken cancellationToken)
    {
        Article? article = await articleRepository.GetAsync(request.ArticleId, cancellationToken);

        if (article is null)
        {
            return Result.Failure(ArticleErrors.NotFound(request.ArticleId));
        }

        Result result = article.Archive();

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
