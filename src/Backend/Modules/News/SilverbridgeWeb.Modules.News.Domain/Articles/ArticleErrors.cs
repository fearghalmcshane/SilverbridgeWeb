using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public static class ArticleErrors
{
    public static Error NotFound(Guid articleId) =>
        Error.NotFound("Articles.NotFound", $"The article with the identifier {articleId} was not found");

    public static Error NotFound(string slug) =>
        Error.NotFound("Articles.NotFound", $"The article with the slug {slug} was not found");

    public static Error CategoryNotFound(Guid categoryId) =>
        Error.NotFound("Articles.CategoryNotFound", $"The category with the identifier {categoryId} was not found");

    public static Error MediaNotFound(Guid mediaId) =>
        Error.NotFound("Articles.MediaNotFound", $"The article media with the identifier {mediaId} was not found");

    public static readonly Error AlreadyPublished = Error.Problem(
        "Articles.AlreadyPublished",
        "The article has already been published");

    public static readonly Error AlreadyArchived = Error.Problem(
        "Articles.AlreadyArchived",
        "The article has already been archived");

    public static readonly Error SlugAlreadyInUse = Error.Problem(
        "Articles.SlugAlreadyInUse",
        "The article slug is already in use");
}
