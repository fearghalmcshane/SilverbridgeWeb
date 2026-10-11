using FluentValidation;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;

namespace SilverbridgeWeb.Modules.Matches.Application.Matches.CaptureMatchCompletion;

public sealed class CaptureMatchCompletionCommandValidator : AbstractValidator<CaptureMatchCompletionCommand>
{
    public CaptureMatchCompletionCommandValidator()
    {
        RuleFor(command => command.Snapshot.ProviderEventId).NotEmpty();
        RuleFor(command => command.Snapshot.ProviderMatchId).NotEmpty();
        RuleFor(command => command.Snapshot.EventType).Equal("match.completed");
        RuleFor(command => command.Snapshot.ApiVersion).NotEmpty();
        RuleFor(command => command.Snapshot.Status).Equal("completed");
        RuleFor(command => command.Snapshot.Sport).NotEmpty();
        RuleFor(command => command.Snapshot.TeamName).NotEmpty();
        RuleFor(command => command.Snapshot.OpponentName).NotEmpty();
        RuleFor(command => command.Snapshot.Result).NotEmpty();
        RuleFor(command => command.Snapshot.TeamScore).NotNull().ChildRules(score =>
        {
            score.RuleFor(value => value.Goals).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Points).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Total).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Display).NotEmpty();
        });
        RuleFor(command => command.Snapshot.OpponentScore).NotNull().ChildRules(score =>
        {
            score.RuleFor(value => value.Goals).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Points).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Total).GreaterThanOrEqualTo(0);
            score.RuleFor(value => value.Display).NotEmpty();
        });
        RuleFor(command => command.OriginalJson).NotEmpty();
    }
}
