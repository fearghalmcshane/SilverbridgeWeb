using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.AddArticleMedia;

internal sealed class AddArticleMediaCommandHandler(IArticleRepository articleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<AddArticleMediaCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddArticleMediaCommand request, CancellationToken cancellationToken)
    {
        Article? article = await articleRepository.GetAsync(request.ArticleId, cancellationToken);

        if (article is null)
        {
            return Result.Failure<Guid>(ArticleErrors.NotFound(request.ArticleId));
        }

        ArticleMedia media = article.AddMedia(request.BlobUrl, request.MediaType, request.AltText, request.DisplayOrder);

        articleRepository.AddMedia(media);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return media.Id;
    }
}
