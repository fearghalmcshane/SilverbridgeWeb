using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public static class ArticleErrors
{
    public static Error TitleRequired => Error.Validation("Article.TitleRequired", "Article title is required.");

    public static Error ContentRequired => Error.Validation("Article.ContentRequired", "Article content is required.");

    public static Error NotFound(Guid id) => Error.NotFound("Article.NotFound", $"Article with ID {id} was not found.");

    public static Error AlreadyPublished => Error.Conflict(
        "Article.AlreadyPublished",
        "Article is already published.");
}
