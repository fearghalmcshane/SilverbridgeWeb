using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetPublishedArticles;

internal sealed class GetPublishedArticlesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPublishedArticlesQuery, GetPublishedArticlesResponse>
{
    public async Task<Result<GetPublishedArticlesResponse>> Handle(
        GetPublishedArticlesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string countSql = """
            SELECT COUNT(*)
            FROM news.articles
            WHERE status = @Status
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(
            countSql,
            new { Status = (int)ArticleStatus.Published });

        const string articlesSql =
            $"""
             SELECT
                 a.id AS {nameof(ArticleListItemResponse.Id)},
                 a.title AS {nameof(ArticleListItemResponse.Title)},
                 a.excerpt AS {nameof(ArticleListItemResponse.Excerpt)},
                 a.featured_image_url AS {nameof(ArticleListItemResponse.FeaturedImage)},
                 a.author_id AS {nameof(ArticleListItemResponse.AuthorId)},
                 a.published_at_utc AS {nameof(ArticleListItemResponse.PublishedAtUtc)}
             FROM news.articles a
             WHERE a.status = @Status
             ORDER BY a.published_at_utc DESC
             LIMIT @PageSize OFFSET @Offset
             """;

        int offset = request.Page * request.PageSize;

        var articlesList = (await connection.QueryAsync<ArticleListItemResponse>(
            articlesSql,
            new { Status = (int)ArticleStatus.Published, request.PageSize, Offset = offset })).ToList();

        // Query categories separately
        if (articlesList.Any())
        {
            var articleIds = articlesList.Select(a => a.Id).ToList();

            const string categoriesSql = """
                SELECT article_id, category_id
                FROM news.article_categories
                WHERE article_id = ANY(@ArticleIds)
                """;

            var categoryMappings = (await connection.QueryAsync<(Guid ArticleId, Guid CategoryId)>(
                categoriesSql,
                new { ArticleIds = articleIds.ToArray() })).ToList();

            var categoryGroups = categoryMappings.GroupBy(c => c.ArticleId).ToDictionary(g => g.Key, g => g.Select(x => x.CategoryId).ToList());

            articlesList = articlesList.Select(a =>
                new ArticleListItemResponse(
                    a.Id,
                    a.Title,
                    a.Excerpt,
                    a.FeaturedImage,
                    a.AuthorId,
                    a.PublishedAtUtc,
                    categoryGroups.TryGetValue(a.Id, out List<Guid>? cats) ? cats : Array.Empty<Guid>())
            ).ToList();
        }

        GetPublishedArticlesResponse response = new(
            articlesList,
            totalCount,
            request.Page,
            request.PageSize);

        return response;
    }
}
