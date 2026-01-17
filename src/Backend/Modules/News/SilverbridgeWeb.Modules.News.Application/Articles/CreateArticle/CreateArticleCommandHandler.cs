using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;

internal sealed class CreateArticleCommandHandler(
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateArticleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        Result<Article> result = Article.Create(
            request.Title,
            request.Content,
            request.AuthorId,
            request.Excerpt,
            request.FeaturedImage,
            request.CategoryIds);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        articleRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
