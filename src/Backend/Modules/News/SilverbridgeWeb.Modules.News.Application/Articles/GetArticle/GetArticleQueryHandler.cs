using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

internal sealed class GetArticleQueryHandler(IArticleRepository articleRepository)
    : IQueryHandler<GetArticleQuery, ArticleResponse?>
{
    public async Task<Result<ArticleResponse?>> Handle(
        GetArticleQuery request,
        CancellationToken cancellationToken)
    {
        Article? article = await articleRepository.GetAsync(request.Id, cancellationToken);

        if (article is null)
        {
            return (ArticleResponse?)null;
        }

        if (!request.IncludeDrafts && article.Status != ArticleStatus.Published)
        {
            return (ArticleResponse?)null;
        }

        ArticleResponse response = new(
            article.Id,
            article.Title,
            article.Content,
            article.Excerpt,
            article.FeaturedImage,
            article.AuthorId,
            article.PublishedAtUtc,
            article.CreatedAtUtc,
            article.UpdatedAtUtc,
            article.Status,
            article.CategoryIds);

        return response;
    }
}
