using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;

/// <summary>
/// Creates a draft match report from an externally captured match completion. "Team" is the club's side
/// and "Opponent" the other side; home/away is derived from <see cref="VenueType"/>.
/// Returns the new article ID, or <c>null</c> when a report already exists for the source match.
/// </summary>
public sealed record CreateMatchReportFromCompletionCommand(
    string SourceProvider,
    string SourceMatchId,
    string TeamName,
    int TeamGoals,
    int TeamPoints,
    string TeamScoreDisplay,
    string OpponentName,
    int OpponentGoals,
    int OpponentPoints,
    string OpponentScoreDisplay,
    string Result,
    string? Competition,
    string? Stage,
    string? VenueName,
    string? VenueType,
    DateTime EndedAtUtc) : ICommand<Guid?>;
