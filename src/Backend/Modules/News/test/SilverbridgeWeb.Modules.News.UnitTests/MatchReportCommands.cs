using SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;

namespace SilverbridgeWeb.Modules.News.UnitTests;

internal static class MatchReportCommands
{
    public static CreateMatchReportFromCompletionCommand Completed() =>
        new(
            "TeamTally",
            "tt-match-1",
            "Silverbridge Harps",
            2,
            2,
            "2-02",
            "Armagh Harps",
            0,
            1,
            "0-01",
            "win",
            "Championship",
            null,
            "Keeley Park",
            "home",
            new DateTime(2026, 10, 11, 19, 0, 0, DateTimeKind.Utc));
}
