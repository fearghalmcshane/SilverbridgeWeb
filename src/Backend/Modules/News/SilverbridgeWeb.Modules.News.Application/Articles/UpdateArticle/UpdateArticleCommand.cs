using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Application.Articles.UpdateArticle;

public sealed record UpdateArticleCommand(
    Guid ArticleId,
    string Title,
    string Slug,
    string Summary,
    string Content,
    Guid CategoryId,
    ArticleType ArticleType,
    string? HomeTeam,
    string? AwayTeam,
    int? HomeGoals,
    int? HomePoints,
    int? AwayGoals,
    int? AwayPoints,
    string? Competition,
    string? Venue,
    DateTime? MatchDateUtc) : ICommand;
