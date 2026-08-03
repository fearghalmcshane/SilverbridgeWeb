using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Presentation.Articles;

internal sealed class CreateArticle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("news/articles", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateArticleCommand(
                request.Title,
                request.Slug,
                request.Summary,
                request.Content,
                request.CategoryId,
                request.AuthorUserId,
                request.AuthorFirstName,
                request.AuthorLastName,
                request.ArticleType,
                request.HomeTeam,
                request.AwayTeam,
                request.HomeGoals,
                request.HomePoints,
                request.AwayGoals,
                request.AwayPoints,
                request.Competition,
                request.Venue,
                request.MatchDateUtc));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.CreateArticles)
        .WithTags(Tags.News);
    }

    internal sealed class Request
    {
        public string Title { get; init; }

        public string Slug { get; init; }

        public string Summary { get; init; }

        public string Content { get; init; }

        public Guid CategoryId { get; init; }

        public Guid AuthorUserId { get; init; }

        public string AuthorFirstName { get; init; }

        public string AuthorLastName { get; init; }

        public ArticleType ArticleType { get; init; }

        public string? HomeTeam { get; init; }

        public string? AwayTeam { get; init; }

        public int? HomeGoals { get; init; }

        public int? HomePoints { get; init; }

        public int? AwayGoals { get; init; }

        public int? AwayPoints { get; init; }

        public string? Competition { get; init; }

        public string? Venue { get; init; }

        public DateTime? MatchDateUtc { get; init; }
    }
}
