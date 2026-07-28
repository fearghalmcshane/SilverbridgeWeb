using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.GetArticles;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class GetArticles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("news/articles", async (
            ISender sender,
            Guid? categoryId,
            int page = 1,
            int pageSize = 15) =>
        {
            Result<GetArticlesResponse> result = await sender.Send(
                new GetArticlesQuery(page, pageSize, categoryId, ArticleStatus.Published, IncludeAllStatuses: false));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.News);

        app.MapGet("news/admin/articles", async (
            ISender sender,
            Guid? categoryId,
            ArticleStatus? status,
            bool includeAllStatuses = true,
            int page = 1,
            int pageSize = 15) =>
        {
            Result<GetArticlesResponse> result = await sender.Send(
                new GetArticlesQuery(page, pageSize, categoryId, status, includeAllStatuses));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.SearchArticles)
        .WithTags(Tags.News);
    }
}
