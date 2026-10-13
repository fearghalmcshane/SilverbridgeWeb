using FluentValidation;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;

internal sealed class CreateMatchReportFromCompletionCommandValidator
    : AbstractValidator<CreateMatchReportFromCompletionCommand>
{
    public CreateMatchReportFromCompletionCommandValidator()
    {
        RuleFor(c => c.SourceProvider).NotEmpty().MaximumLength(50);
        RuleFor(c => c.SourceMatchId).NotEmpty().MaximumLength(200);
        RuleFor(c => c.TeamName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.OpponentName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.TeamGoals).GreaterThanOrEqualTo(0);
        RuleFor(c => c.TeamPoints).GreaterThanOrEqualTo(0);
        RuleFor(c => c.OpponentGoals).GreaterThanOrEqualTo(0);
        RuleFor(c => c.OpponentPoints).GreaterThanOrEqualTo(0);
        RuleFor(c => c.TeamScoreDisplay).NotEmpty().MaximumLength(50);
        RuleFor(c => c.OpponentScoreDisplay).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Competition).MaximumLength(200);
        RuleFor(c => c.Stage).MaximumLength(200);
        RuleFor(c => c.VenueName).MaximumLength(200);
    }
}
