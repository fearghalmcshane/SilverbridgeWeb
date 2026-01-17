using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.GetArticle;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class GetArticle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("news/articles/{id}", async (Guid id, ISender sender, bool includeDrafts = false) =>
        {
            Result<ArticleResponse?> result = await sender.Send(new GetArticleQuery(id, includeDrafts));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.News);
    }
}
