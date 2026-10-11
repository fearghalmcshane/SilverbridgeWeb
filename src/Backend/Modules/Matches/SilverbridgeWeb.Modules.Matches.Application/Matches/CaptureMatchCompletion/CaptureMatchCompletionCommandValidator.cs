using FluentValidation;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;

namespace SilverbridgeWeb.Modules.Matches.Application.Matches.CaptureMatchCompletion;

public sealed class CaptureMatchCompletionCommandValidator : AbstractValidator<CaptureMatchCompletionCommand>
{
    public CaptureMatchCompletionCommandValidator()
    {
        RuleFor(command => command.Snapshot.ProviderEventId).NotEmpty().MaximumLength(300);
        RuleFor(command => command.Snapshot.ProviderMatchId).NotEmpty().MaximumLength(300);
        RuleFor(command => command.Snapshot.EventType).Equal("match.completed").MaximumLength(100);
        RuleFor(command => command.Snapshot.ApiVersion).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Snapshot.Status).Equal("completed", StringComparer.OrdinalIgnoreCase).MaximumLength(100);
        RuleFor(command => command.Snapshot.Sport).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Snapshot.Competition).MaximumLength(300);
        RuleFor(command => command.Snapshot.Stage).MaximumLength(300);
        RuleFor(command => command.Snapshot.VenueName).MaximumLength(300);
        RuleFor(command => command.Snapshot.VenueType).MaximumLength(100);
        RuleFor(command => command.Snapshot.TeamName).NotEmpty().MaximumLength(300);
        RuleFor(command => command.Snapshot.OpponentName).NotEmpty().MaximumLength(300);
        RuleFor(command => command.Snapshot.Result).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Snapshot.Summary).MaximumLength(4000);
        RuleFor(command => command.Snapshot.LiveUrl!.OriginalString)
            .MaximumLength(2000)
            .When(command => command.Snapshot.LiveUrl is not null);
        RuleFor(command => command.Snapshot.TeamScore).NotNull().ChildRules(score =>
        {
            score.RuleFor(value => value.Goals).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Points).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Total).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Display).NotEmpty().MaximumLength(100);
        });
        RuleFor(command => command.Snapshot.OpponentScore).NotNull().ChildRules(score =>
        {
            score.RuleFor(value => value.Goals).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Points).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Total).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Display).NotEmpty().MaximumLength(100);
        });
        RuleFor(command => command.OriginalJson).NotEmpty();
    }
}
