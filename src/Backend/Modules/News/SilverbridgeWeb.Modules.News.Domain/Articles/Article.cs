using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.News.Domain.Articles;

public sealed class Article : Entity
{
    private readonly List<Guid> _categoryIds = [];

    private Article()
    {
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string Content { get; private set; }

    public string? Excerpt { get; private set; }

    public string? FeaturedImage { get; private set; }

    public Guid AuthorId { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public ArticleStatus Status { get; private set; }

    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();

    public static Result<Article> Create(
        string title,
        string content,
        Guid authorId,
        string? excerpt = null,
        string? featuredImage = null,
        IReadOnlyCollection<Guid>? categoryIds = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Article>(ArticleErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<Article>(ArticleErrors.ContentRequired);
        }

        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Excerpt = excerpt,
            FeaturedImage = featuredImage,
            AuthorId = authorId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Status = ArticleStatus.Draft
        };

        if (categoryIds is not null)
        {
            foreach (Guid categoryId in categoryIds)
            {
                article._categoryIds.Add(categoryId);
            }
        }

        article.Raise(new ArticleCreatedDomainEvent(article.Id, article.Title, article.AuthorId));

        return article;
    }

    public Result Publish(DateTime utcNow)
    {
        if (Status == ArticleStatus.Published)
        {
            return Result.Failure(ArticleErrors.AlreadyPublished);
        }

        Status = ArticleStatus.Published;
        PublishedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        Raise(new ArticlePublishedDomainEvent(Id, Title, AuthorId, PublishedAtUtc.Value));

        return Result.Success();
    }

    public Result Update(
        string title,
        string content,
        string? excerpt = null,
        string? featuredImage = null,
        IReadOnlyCollection<Guid>? categoryIds = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(ArticleErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure(ArticleErrors.ContentRequired);
        }

        Title = title;
        Content = content;
        Excerpt = excerpt;
        FeaturedImage = featuredImage;
        UpdatedAtUtc = DateTime.UtcNow;

        if (categoryIds is not null)
        {
            _categoryIds.Clear();
            foreach (Guid categoryId in categoryIds)
            {
                _categoryIds.Add(categoryId);
            }
        }

        Raise(new ArticleUpdatedDomainEvent(Id, Title));

        return Result.Success();
    }

    public void Archive()
    {
        if (Status == ArticleStatus.Archived)
        {
            return;
        }

        Status = ArticleStatus.Archived;
        UpdatedAtUtc = DateTime.UtcNow;

        Raise(new ArticleArchivedDomainEvent(Id));
    }
}
