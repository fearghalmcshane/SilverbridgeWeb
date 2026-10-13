using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.News.Domain.Articles;
using SilverbridgeWeb.Modules.News.Domain.Categories;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;

internal sealed class CreateMatchReportFromCompletionCommandHandler(
    IDateTimeProvider dateTimeProvider,
    ICategoryRepository categoryRepository,
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMatchReportFromCompletionCommand, Guid?>
{
    private const int MaxSlugAttempts = 50;

    public async Task<Result<Guid?>> Handle(
        CreateMatchReportFromCompletionCommand request,
        CancellationToken cancellationToken)
    {
        if (await articleRepository.MatchReportExistsForSourceAsync(
                request.SourceProvider,
                request.SourceMatchId,
                cancellationToken))
        {
            return Result.Success<Guid?>(null);
        }

        Category? category = await categoryRepository.GetAsync(WellKnownCategories.MatchReportsId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<Guid?>(ArticleErrors.CategoryNotFound(WellKnownCategories.MatchReportsId));
        }

        MatchReportDraft draft = MatchReportDraftComposer.Compose(request);

        string? slug = await ResolveUniqueSlugAsync(draft.Slug, cancellationToken);
        if (slug is null)
        {
            return Result.Failure<Guid?>(ArticleErrors.SlugAlreadyInUse);
        }

        Result<Article> created = Article.Create(
            category,
            SystemAuthors.TeamTally.UserId,
            SystemAuthors.TeamTally.FirstName,
            SystemAuthors.TeamTally.LastName,
            draft.Title,
            slug,
            draft.Summary,
            draft.Content,
            ArticleType.MatchReport,
            dateTimeProvider.UtcNow);
        if (created.IsFailure)
        {
            return Result.Failure<Guid?>(created.Error);
        }

        Article article = created.Value;
        article.SetMatchReportDetails(
            draft.HomeTeam,
            draft.AwayTeam,
            draft.HomeGoals,
            draft.HomePoints,
            draft.AwayGoals,
            draft.AwayPoints,
            string.IsNullOrWhiteSpace(request.Competition) ? null : request.Competition.Trim(),
            string.IsNullOrWhiteSpace(request.VenueName) ? null : request.VenueName.Trim(),
            request.EndedAtUtc);

        Result linked = article.LinkMatchReportSource(request.SourceProvider, request.SourceMatchId);
        if (linked.IsFailure)
        {
            return Result.Failure<Guid?>(linked.Error);
        }

        articleRepository.Insert(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return article.Id;
    }

    private async Task<string?> ResolveUniqueSlugAsync(string baseSlug, CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= MaxSlugAttempts; attempt++)
        {
            string candidate = MatchReportDraftComposer.WithSlugSuffix(baseSlug, attempt);
            if (!await articleRepository.IsSlugInUseAsync(candidate, null, cancellationToken))
            {
                return candidate;
            }
        }

        return null;
    }
}
