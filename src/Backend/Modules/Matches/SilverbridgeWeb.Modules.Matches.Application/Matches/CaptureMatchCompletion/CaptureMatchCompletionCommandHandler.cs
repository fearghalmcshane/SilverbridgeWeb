using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;

namespace SilverbridgeWeb.Modules.Matches.Application.Matches.CaptureMatchCompletion;

internal sealed class CaptureMatchCompletionCommandHandler(IMatchCompletionRepository repository)
    : ICommandHandler<CaptureMatchCompletionCommand, MatchCompletionCaptureOutcome>
{
    public async Task<Result<MatchCompletionCaptureOutcome>> Handle(
        CaptureMatchCompletionCommand request,
        CancellationToken cancellationToken)
    {
        var completion = MatchCompletion.Create(request.Snapshot, request.OriginalJson);
        MatchCompletionCaptureOutcome outcome = await repository.CaptureAsync(completion, cancellationToken);

        return outcome == MatchCompletionCaptureOutcome.EventIdConflict
            ? Result.Failure<MatchCompletionCaptureOutcome>(MatchCompletionErrors.ProviderEventIdConflict)
            : Result.Success(outcome);
    }
}
