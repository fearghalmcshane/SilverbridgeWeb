using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.PublishArticle;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class PublishArticle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("news/articles/{id}/publish", async (Guid id, ISender sender) =>
        {
            Result result = await sender.Send(new PublishArticleCommand(id));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyArticles)
        .WithTags(Tags.News);
    }
}
