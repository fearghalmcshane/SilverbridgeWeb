using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.UpdateArticle;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class UpdateArticle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("news/articles/{id}", async (Guid id, Request request, ISender sender) =>
        {
            Result result = await sender.Send(new UpdateArticleCommand(
                id,
                request.Title,
                request.Slug,
                request.Summary,
                request.Content,
                request.CategoryId));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.UpdateArticles)
        .WithTags(Tags.News);
    }

    internal sealed class Request
    {
        public string Title { get; init; }

        public string Slug { get; init; }

        public string Summary { get; init; }

        public string Content { get; init; }

        public Guid CategoryId { get; init; }
    }
}
