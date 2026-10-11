using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;

namespace SilverbridgeWeb.Modules.Matches.Application.Matches.CaptureMatchCompletion;

public sealed record CaptureMatchCompletionCommand(
    MatchCompletionSnapshot Snapshot,
    string OriginalJson) : ICommand<MatchCompletionCaptureOutcome>;
