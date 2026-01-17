using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class CreateArticle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("news/articles", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateArticleCommand(
                request.Title,
                request.Content,
                request.AuthorId,
                request.Excerpt,
                request.FeaturedImageUrl,
                request.CategoryIds));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyArticles)
        .WithTags(Tags.News);
    }

    internal sealed class Request
    {
        public string Title { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;

        public Guid AuthorId { get; init; }

        public string? Excerpt { get; init; }

        public string? FeaturedImageUrl { get; init; }

        public IReadOnlyCollection<Guid>? CategoryIds { get; init; }
    }
}
