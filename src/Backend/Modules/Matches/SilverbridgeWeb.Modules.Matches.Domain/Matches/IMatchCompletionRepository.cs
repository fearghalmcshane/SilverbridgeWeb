namespace SilverbridgeWeb.Modules.Matches.Domain.Matches;

public interface IMatchCompletionRepository
{
    Task<MatchCompletionCaptureOutcome> CaptureAsync(
        MatchCompletion completion,
        CancellationToken cancellationToken);
}
