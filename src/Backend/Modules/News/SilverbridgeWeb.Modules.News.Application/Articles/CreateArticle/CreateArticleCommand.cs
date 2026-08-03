using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;

public sealed record CreateArticleCommand(
    string Title,
    string Slug,
    string Summary,
    string Content,
    Guid CategoryId,
    Guid AuthorUserId,
    string AuthorFirstName,
    string AuthorLastName,
    ArticleType ArticleType,
    string? HomeTeam,
    string? AwayTeam,
    int? HomeGoals,
    int? HomePoints,
    int? AwayGoals,
    int? AwayPoints,
    string? Competition,
    string? Venue,
    DateTime? MatchDateUtc) : ICommand<Guid>;
