using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Matches.Domain.Matches;

public static class MatchCompletionErrors
{
    public static readonly Error ProviderEventIdConflict =
        Error.Conflict(
            "Matches.ProviderEventIdConflict",
            "The provider event ID is already associated with another match.");
}
