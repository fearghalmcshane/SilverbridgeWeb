using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

internal sealed class GetArticleQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetArticleQuery, ArticleResponse>
{
    public async Task<Result<ArticleResponse>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        if (request.ArticleId.HasValue && request.ArticleId.Value != Guid.Empty)
        {
            ArticleResponse? article = await GetByIdAsync(connection, request.ArticleId.Value);

            return article is null
                ? Result.Failure<ArticleResponse>(ArticleErrors.NotFound(request.ArticleId.Value))
                : article;
        }

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            ArticleResponse? article = await GetBySlugAsync(connection, request.Slug);

            return article is null
                ? Result.Failure<ArticleResponse>(ArticleErrors.NotFound(request.Slug))
                : article;
        }

        return Result.Failure<ArticleResponse>(Error.Problem(
            "Articles.InvalidQuery",
            "Either an article identifier or slug must be provided"));
    }

    private static async Task<ArticleResponse?> GetByIdAsync(DbConnection connection, Guid articleId)
    {
        const string sql =
            $"""
             SELECT
                 a.id AS {nameof(ArticleResponse.Id)},
                 a.category_id AS {nameof(ArticleResponse.CategoryId)},
                 c.name AS {nameof(ArticleResponse.CategoryName)},
                 a.author_user_id AS {nameof(ArticleResponse.AuthorUserId)},
                 a.author_first_name AS {nameof(ArticleResponse.AuthorFirstName)},
                 a.author_last_name AS {nameof(ArticleResponse.AuthorLastName)},
                 a.title AS {nameof(ArticleResponse.Title)},
                 a.slug AS {nameof(ArticleResponse.Slug)},
                 a.summary AS {nameof(ArticleResponse.Summary)},
                 a.content AS {nameof(ArticleResponse.Content)},
                 a.status AS {nameof(ArticleResponse.Status)},
                 a.published_at_utc AS {nameof(ArticleResponse.PublishedAtUtc)},
                 a.created_at_utc AS {nameof(ArticleResponse.CreatedAtUtc)},
                 a.updated_at_utc AS {nameof(ArticleResponse.UpdatedAtUtc)},
                 m.id AS {nameof(ArticleMediaResponse.MediaId)},
                 m.blob_url AS {nameof(ArticleMediaResponse.BlobUrl)},
                 m.media_type AS {nameof(ArticleMediaResponse.MediaType)},
                 m.alt_text AS {nameof(ArticleMediaResponse.AltText)},
                 m.display_order AS {nameof(ArticleMediaResponse.DisplayOrder)}
             FROM news.articles a
             INNER JOIN news.categories c ON c.id = a.category_id
             LEFT JOIN news.article_media m ON m.article_id = a.id
             WHERE a.id = @ArticleId
             ORDER BY m.display_order, m.id
             """;

        return await GetArticleAsync(connection, sql, new { ArticleId = articleId });
    }

    private static async Task<ArticleResponse?> GetBySlugAsync(DbConnection connection, string slug)
    {
        const string sql =
            $"""
             SELECT
                 a.id AS {nameof(ArticleResponse.Id)},
                 a.category_id AS {nameof(ArticleResponse.CategoryId)},
                 c.name AS {nameof(ArticleResponse.CategoryName)},
                 a.author_user_id AS {nameof(ArticleResponse.AuthorUserId)},
                 a.author_first_name AS {nameof(ArticleResponse.AuthorFirstName)},
                 a.author_last_name AS {nameof(ArticleResponse.AuthorLastName)},
                 a.title AS {nameof(ArticleResponse.Title)},
                 a.slug AS {nameof(ArticleResponse.Slug)},
                 a.summary AS {nameof(ArticleResponse.Summary)},
                 a.content AS {nameof(ArticleResponse.Content)},
                 a.status AS {nameof(ArticleResponse.Status)},
                 a.published_at_utc AS {nameof(ArticleResponse.PublishedAtUtc)},
                 a.created_at_utc AS {nameof(ArticleResponse.CreatedAtUtc)},
                 a.updated_at_utc AS {nameof(ArticleResponse.UpdatedAtUtc)},
                 m.id AS {nameof(ArticleMediaResponse.MediaId)},
                 m.blob_url AS {nameof(ArticleMediaResponse.BlobUrl)},
                 m.media_type AS {nameof(ArticleMediaResponse.MediaType)},
                 m.alt_text AS {nameof(ArticleMediaResponse.AltText)},
                 m.display_order AS {nameof(ArticleMediaResponse.DisplayOrder)}
             FROM news.articles a
             INNER JOIN news.categories c ON c.id = a.category_id
             LEFT JOIN news.article_media m ON m.article_id = a.id
             WHERE a.slug = @Slug AND a.status = @PublishedStatus
             ORDER BY m.display_order, m.id
             """;

        return await GetArticleAsync(connection, sql, new { Slug = slug, PublishedStatus = (int)ArticleStatus.Published });
    }

    private static async Task<ArticleResponse?> GetArticleAsync(DbConnection connection, string sql, object parameters)
    {
        Dictionary<Guid, ArticleResponse> articles = [];

        await connection.QueryAsync<ArticleResponse, ArticleMediaResponse?, ArticleResponse>(
            sql,
            (article, media) =>
            {
                if (articles.TryGetValue(article.Id, out ArticleResponse? existingArticle))
                {
                    article = existingArticle;
                }
                else
                {
                    articles.Add(article.Id, article);
                }

                if (media is not null)
                {
                    article.Media.Add(media);
                }

                return article;
            },
            parameters,
            splitOn: nameof(ArticleMediaResponse.MediaId));

        return articles.Values.SingleOrDefault();
    }
}
