using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class GetArticle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("news/articles/{slug}", async (string slug, ISender sender) =>
        {
            Result<ArticleResponse> result = await sender.Send(new GetArticleQuery(null, slug));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.News);

        app.MapGet("news/articles/id/{id}", async (Guid id, ISender sender) =>
        {
            Result<ArticleResponse> result = await sender.Send(new GetArticleQuery(id, null));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetArticles)
        .WithTags(Tags.News);
    }
}
