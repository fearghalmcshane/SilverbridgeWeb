using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SilverbridgeWeb.WebUI.Services.ApiClients;

internal sealed class NewsApiClient(HttpClient httpClient) : IApiClient
{
    public string BaseEndpoint => "/news";

    public async Task<NewsArticleListResponse?> GetArticlesAsync(
        int page,
        int pageSize,
        Guid? categoryId,
        bool includeAllStatuses,
        CancellationToken cancellationToken = default)
    {
        string path = includeAllStatuses ? $"{BaseEndpoint}/admin/articles" : $"{BaseEndpoint}/articles";

        string url = BuildUrl(path, [
            ("page", page.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture)),
            ("categoryId", categoryId?.ToString()),
            ("includeAllStatuses", includeAllStatuses.ToString().ToLowerInvariant())
        ]);

        return await httpClient.GetFromJsonAsync<NewsArticleListResponse>(url, cancellationToken);
    }

    public async Task<NewsArticleResponse?> GetArticleAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<NewsArticleResponse>(
            $"{BaseEndpoint}/articles/{Uri.EscapeDataString(slug)}",
            cancellationToken);
    }

    public async Task<NewsArticleResponse?> GetArticleAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<NewsArticleResponse>(
            $"{BaseEndpoint}/articles/id/{articleId}",
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<NewsCategoryResponse>?> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<NewsCategoryResponse>>(
            $"{BaseEndpoint}/categories",
            cancellationToken);
    }

    public async Task<Guid?> CreateArticleAsync(
        CreateNewsArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{BaseEndpoint}/articles",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task UpdateArticleAsync(
        Guid articleId,
        UpdateNewsArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync(
            $"{BaseEndpoint}/articles/{articleId}",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task PublishArticleAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PutAsync(
            $"{BaseEndpoint}/articles/{articleId}/publish",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task ArchiveArticleAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PutAsync(
            $"{BaseEndpoint}/articles/{articleId}/archive",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<Guid?> UploadArticleMediaAsync(
        Guid articleId,
        Stream fileContent,
        string fileName,
        string contentType,
        string? altText,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        using MultipartFormDataContent form = [];
        using StreamContent streamContent = new(fileContent);
        using StringContent displayOrderContent = new(displayOrder.ToString(CultureInfo.InvariantCulture));
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        form.Add(streamContent, "file", fileName);
        form.Add(displayOrderContent, "displayOrder");

        if (!string.IsNullOrWhiteSpace(altText))
        {
            using StringContent altTextContent = new(altText);
            form.Add(altTextContent, "altText");
        }

        HttpResponseMessage response = await httpClient.PostAsync(
            $"{BaseEndpoint}/articles/{articleId}/media",
            form,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    public async Task<Guid?> CreateCategoryAsync(
        CreateNewsCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"{BaseEndpoint}/categories",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
    }

    private static string BuildUrl(string path, (string Key, string? Value)[] parameters)
    {
        List<string> queryParams = [];

        foreach ((string key, string? value) in parameters)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                queryParams.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }
        }

        return queryParams.Count > 0 ? $"{path}?{string.Join('&', queryParams)}" : path;
    }
}

internal enum NewsArticleStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

internal sealed record NewsArticleListResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<NewsArticleSummaryResponse> Articles);

internal sealed record NewsArticleSummaryResponse(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Summary,
    NewsArticleStatus Status,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    string AuthorFirstName,
    string AuthorLastName);

internal sealed record NewsArticleResponse(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid AuthorUserId,
    string AuthorFirstName,
    string AuthorLastName,
    string Title,
    string Slug,
    string Summary,
    string Content,
    NewsArticleStatus Status,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc)
{
    public List<NewsArticleMediaResponse> Media { get; init; } = [];
}

internal sealed record NewsArticleMediaResponse(
    Guid MediaId,
    string BlobUrl,
    string MediaType,
    string? AltText,
    int DisplayOrder);

internal sealed record NewsCategoryResponse(Guid Id, string Name, bool IsArchived);

internal sealed record CreateNewsArticleRequest(
    string Title,
    string Slug,
    string Summary,
    string Content,
    Guid CategoryId,
    Guid AuthorUserId,
    string AuthorFirstName,
    string AuthorLastName);

internal sealed record UpdateNewsArticleRequest(
    string Title,
    string Slug,
    string Summary,
    string Content,
    Guid CategoryId);

internal sealed record CreateNewsCategoryRequest(string Name);
