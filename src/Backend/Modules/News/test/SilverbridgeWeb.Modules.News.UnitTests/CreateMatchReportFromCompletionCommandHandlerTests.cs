using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.UnitTests;

public sealed class CreateMatchReportFromCompletionCommandHandlerTests
{
    private static readonly DateTime Now = new(2026, 10, 11, 21, 0, 0, DateTimeKind.Utc);

    private readonly FakeArticleRepository _articles = new();
    private readonly FakeCategoryRepository _categories = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task Handle_CreatesDraftMatchReportAuthoredByTeamTally()
    {
        Result<Guid?> result = await CreateHandler().Handle(
            MatchReportCommands.Completed() with { VenueType = "away" },
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Article article = Assert.Single(_articles.Inserted);
        Assert.Equal(article.Id, result.Value);
        Assert.Equal(ArticleStatus.Draft, article.Status);
        Assert.Null(article.PublishedAtUtc);
        Assert.Equal(ArticleType.MatchReport, article.ArticleType);
        Assert.Equal(WellKnownCategories.MatchReportsId, article.CategoryId);
        Assert.Equal(SystemAuthors.TeamTally.UserId, article.AuthorUserId);
        Assert.Equal("Team", article.AuthorFirstName);
        Assert.Equal("Tally", article.AuthorLastName);
        Assert.Equal("Silverbridge Harps beat Armagh Harps in Championship", article.Title);
        Assert.Equal("silverbridge-harps-v-armagh-harps-2026-10-11", article.Slug);
        Assert.Equal(Now, article.CreatedAtUtc);

        MatchReportDetails details = Assert.IsType<MatchReportDetails>(article.MatchReportDetails);
        Assert.Equal("Armagh Harps", details.HomeTeam);
        Assert.Equal("Silverbridge Harps", details.AwayTeam);
        Assert.Equal((0, 1, 2, 2), (details.HomeGoals, details.HomePoints, details.AwayGoals, details.AwayPoints));
        Assert.Equal("Championship", details.Competition);
        Assert.Equal("Keeley Park", details.Venue);
        Assert.Equal(new DateTime(2026, 10, 11, 19, 0, 0, DateTimeKind.Utc), details.MatchDateUtc);
        Assert.Equal("TeamTally", details.SourceProvider);
        Assert.Equal("tt-match-1", details.SourceMatchId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_DoesNothing_WhenReportAlreadyExistsForSourceMatch()
    {
        _articles.ExistingSources.Add(("TeamTally", "tt-match-1"));

        Result<Guid?> result = await CreateHandler().Handle(
            MatchReportCommands.Completed(),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Empty(_articles.Inserted);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_Fails_WhenMatchReportsCategoryIsMissing()
    {
        _categories.Category = null;

        Result<Guid?> result = await CreateHandler().Handle(
            MatchReportCommands.Completed(),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ArticleErrors.CategoryNotFound(WellKnownCategories.MatchReportsId).Code, result.Error.Code);
        Assert.Empty(_articles.Inserted);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_AppendsSuffix_WhenSlugIsAlreadyInUse()
    {
        _articles.UsedSlugs.Add("silverbridge-harps-v-armagh-harps-2026-10-11");
        _articles.UsedSlugs.Add("silverbridge-harps-v-armagh-harps-2026-10-11-2");

        Result<Guid?> result = await CreateHandler().Handle(
            MatchReportCommands.Completed(),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("silverbridge-harps-v-armagh-harps-2026-10-11-3", Assert.Single(_articles.Inserted).Slug);
    }

    private CreateMatchReportFromCompletionCommandHandler CreateHandler() =>
        new(new FixedDateTimeProvider(), _categories, _articles, _unitOfWork);

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => Now;
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public Category? Category { get; set; } = CreateMatchReportsCategory();

        public Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == WellKnownCategories.MatchReportsId ? Category : null);

        public void Insert(Category category) => throw new NotSupportedException();

        private static Category CreateMatchReportsCategory()
        {
            var category = Category.Create("Match Reports");
            typeof(Category).GetProperty(nameof(Category.Id))!.SetValue(category, WellKnownCategories.MatchReportsId);
            return category;
        }
    }

    private sealed class FakeArticleRepository : IArticleRepository
    {
        public List<Article> Inserted { get; } = [];

        public HashSet<(string Provider, string MatchId)> ExistingSources { get; } = [];

        public HashSet<string> UsedSlugs { get; } = [];

        public Task<Article?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public void Insert(Article article) => Inserted.Add(article);

        public void AddMedia(ArticleMedia media) => throw new NotSupportedException();

        public void TrackMatchReportDetails(MatchReportDetails details) => throw new NotSupportedException();

        public void RemoveMatchReportDetails(MatchReportDetails details) => throw new NotSupportedException();

        public Task<bool> MatchReportExistsForSourceAsync(
            string sourceProvider,
            string sourceMatchId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingSources.Contains((sourceProvider, sourceMatchId)));

        public Task<bool> IsSlugInUseAsync(
            string slug,
            Guid? excludeArticleId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(UsedSlugs.Contains(slug));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
