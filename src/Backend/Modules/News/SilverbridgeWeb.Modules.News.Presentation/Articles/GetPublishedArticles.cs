using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.GetPublishedArticles;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class GetPublishedArticles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("news/articles", async (ISender sender, int page = 0, int pageSize = 10) =>
        {
            Result<GetPublishedArticlesResponse> result = await sender.Send(new GetPublishedArticlesQuery(page, pageSize));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.News);
    }
}
