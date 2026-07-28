using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateArticle;

public sealed record CreateArticleCommand(
    string Title,
    string Slug,
    string Summary,
    string Content,
    Guid CategoryId,
    Guid AuthorUserId,
    string AuthorFirstName,
    string AuthorLastName) : ICommand<Guid>;
