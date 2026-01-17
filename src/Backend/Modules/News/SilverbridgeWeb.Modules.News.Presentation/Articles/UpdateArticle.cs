using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
                request.Content,
                request.Excerpt,
                request.FeaturedImageUrl,
                request.CategoryIds));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyArticles)
        .WithTags(Tags.News);
    }

    internal sealed class Request
    {
        public string Title { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;

        public string? Excerpt { get; init; }

        public string? FeaturedImageUrl { get; init; }

        public IReadOnlyCollection<Guid>? CategoryIds { get; init; }
    }
}
