using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.GetArticles;

internal sealed class GetArticlesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetArticlesQuery, GetArticlesResponse>
{
    public async Task<Result<GetArticlesResponse>> Handle(GetArticlesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        int page = request.Page <= 0 ? 1 : request.Page;
        int pageSize = request.PageSize <= 0 ? 15 : Math.Min(request.PageSize, 100);

        var parameters = new GetArticlesParameters(
            request.CategoryId,
            request.Status is null ? null : (int)request.Status.Value,
            request.IncludeAllStatuses,
            (int)ArticleStatus.Published,
            pageSize,
            (page - 1) * pageSize);

        IReadOnlyCollection<ArticleSummaryResponse> articles = await GetArticlesAsync(connection, parameters);
        int totalCount = await CountArticlesAsync(connection, parameters);

        return new GetArticlesResponse(page, pageSize, totalCount, articles);
    }

    private static async Task<IReadOnlyCollection<ArticleSummaryResponse>> GetArticlesAsync(
        DbConnection connection,
        GetArticlesParameters parameters)
    {
        const string sql =
            $"""
             SELECT
                 a.id AS {nameof(ArticleSummaryResponse.Id)},
                 a.category_id AS {nameof(ArticleSummaryResponse.CategoryId)},
                 c.name AS {nameof(ArticleSummaryResponse.CategoryName)},
                 a.title AS {nameof(ArticleSummaryResponse.Title)},
                 a.slug AS {nameof(ArticleSummaryResponse.Slug)},
                 a.summary AS {nameof(ArticleSummaryResponse.Summary)},
                 a.status AS {nameof(ArticleSummaryResponse.Status)},
                 a.published_at_utc AS {nameof(ArticleSummaryResponse.PublishedAtUtc)},
                 a.created_at_utc AS {nameof(ArticleSummaryResponse.CreatedAtUtc)},
                 a.updated_at_utc AS {nameof(ArticleSummaryResponse.UpdatedAtUtc)},
                 a.author_first_name AS {nameof(ArticleSummaryResponse.AuthorFirstName)},
                 a.author_last_name AS {nameof(ArticleSummaryResponse.AuthorLastName)}
             FROM news.articles a
             INNER JOIN news.categories c ON c.id = a.category_id
             WHERE
                 (@CategoryId IS NULL OR a.category_id = @CategoryId) AND
                 ((@IncludeAllStatuses = FALSE AND a.status = @PublishedStatus) OR
                 (@IncludeAllStatuses = TRUE AND (@Status IS NULL OR a.status = @Status)))
             ORDER BY COALESCE(a.published_at_utc, a.created_at_utc) DESC, a.title
             OFFSET @Skip
             LIMIT @Take
             """;

        List<ArticleSummaryResponse> articles = (await connection.QueryAsync<ArticleSummaryResponse>(sql, parameters)).AsList();

        return articles;
    }

    private static async Task<int> CountArticlesAsync(DbConnection connection, GetArticlesParameters parameters)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM news.articles a
            WHERE
                (@CategoryId IS NULL OR a.category_id = @CategoryId) AND
                ((@IncludeAllStatuses = FALSE AND a.status = @PublishedStatus) OR
                (@IncludeAllStatuses = TRUE AND (@Status IS NULL OR a.status = @Status)))
            """;

        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    private sealed record GetArticlesParameters(
        Guid? CategoryId,
        int? Status,
        bool IncludeAllStatuses,
        int PublishedStatus,
        int Take,
        int Skip);
}
