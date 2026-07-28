using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class Article : Entity
{
    private readonly List<ArticleMedia> _media = [];

    private Article()
    {
    }

    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }

    public Guid AuthorUserId { get; private set; }

    public string AuthorFirstName { get; private set; }

    public string AuthorLastName { get; private set; }

    public string Title { get; private set; }

    public string Slug { get; private set; }

    public string Summary { get; private set; }

    public string Content { get; private set; }

    public ArticleStatus Status { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ArticleMedia> Media => _media.AsReadOnly();

    public static Result<Article> Create(
        Category category,
        Guid authorUserId,
        string authorFirstName,
        string authorLastName,
        string title,
        string slug,
        string summary,
        string content,
        DateTime utcNow)
    {
        var article = new Article
        {
            Id = Ulid.NewUlid().ToGuid(),
            CategoryId = category.Id,
            AuthorUserId = authorUserId,
            AuthorFirstName = authorFirstName,
            AuthorLastName = authorLastName,
            Title = title,
            Slug = slug,
            Summary = summary,
            Content = content,
            Status = ArticleStatus.Draft,
            CreatedAtUtc = utcNow
        };

        article.Raise(new ArticleCreatedDomainEvent(article.Id));

        return article;
    }

    public void Update(
        string title,
        string slug,
        string summary,
        string content,
        Guid categoryId,
        DateTime utcNow)
    {
        Title = title;
        Slug = slug;
        Summary = summary;
        Content = content;
        CategoryId = categoryId;
        UpdatedAtUtc = utcNow;

        Raise(new ArticleUpdatedDomainEvent(Id));
    }

    public Result Publish(DateTime utcNow)
    {
        if (Status == ArticleStatus.Published)
        {
            return Result.Failure(ArticleErrors.AlreadyPublished);
        }

        if (Status == ArticleStatus.Archived)
        {
            return Result.Failure(ArticleErrors.AlreadyArchived);
        }

        Status = ArticleStatus.Published;
        PublishedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        Raise(new ArticlePublishedDomainEvent(Id, Title, Summary, Slug, CategoryId, utcNow));

        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == ArticleStatus.Archived)
        {
            return Result.Failure(ArticleErrors.AlreadyArchived);
        }

        Status = ArticleStatus.Archived;

        Raise(new ArticleArchivedDomainEvent(Id));

        return Result.Success();
    }

    public ArticleMedia AddMedia(Uri blobUrl, string mediaType, string? altText, int displayOrder)
    {
        return AddMedia(blobUrl.ToString(), mediaType, altText, displayOrder);
    }

    public ArticleMedia AddMedia(string blobUrl, string mediaType, string? altText, int displayOrder)
    {
        var media = ArticleMedia.Create(Id, blobUrl, mediaType, altText, displayOrder);

        _media.Add(media);

        Raise(new ArticleMediaAddedDomainEvent(Id, media.Id));

        return media;
    }

    public Result RemoveMedia(Guid mediaId)
    {
        ArticleMedia? media = _media.SingleOrDefault(m => m.Id == mediaId);

        if (media is null)
        {
            return Result.Failure(ArticleErrors.MediaNotFound(mediaId));
        }

        _media.Remove(media);

        return Result.Success();
    }
}
