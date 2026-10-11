using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Matches.Domain.Matches;

public sealed class MatchCompletion : Entity
{
    private MatchCompletion()
    {
        ProviderEventId = string.Empty;
        ProviderMatchId = string.Empty;
        EventType = string.Empty;
        ApiVersion = string.Empty;
        Status = string.Empty;
        Sport = string.Empty;
        TeamName = string.Empty;
        TeamScoreDisplay = string.Empty;
        OpponentName = string.Empty;
        OpponentScoreDisplay = string.Empty;
        Result = string.Empty;
        OriginalJson = string.Empty;
    }

    private MatchCompletion(MatchCompletionSnapshot snapshot, string originalJson)
    {
        Id = Ulid.NewUlid().ToGuid();
        ProviderEventId = snapshot.ProviderEventId;
        ProviderMatchId = snapshot.ProviderMatchId;
        EventType = snapshot.EventType;
        ApiVersion = snapshot.ApiVersion;
        ProviderCreatedAtUtc = snapshot.ProviderCreatedAtUtc;
        ReceivedAtUtc = snapshot.ReceivedAtUtc;
        Status = snapshot.Status;
        Sport = snapshot.Sport;
        Competition = snapshot.Competition;
        Stage = snapshot.Stage;
        EndedAtUtc = snapshot.EndedAtUtc;
        VenueName = snapshot.VenueName;
        VenueType = snapshot.VenueType;
        TeamName = snapshot.TeamName;
        TeamGoals = snapshot.TeamScore.Goals;
        TeamPoints = snapshot.TeamScore.Points;
        TeamTotal = snapshot.TeamScore.Total;
        TeamScoreDisplay = snapshot.TeamScore.Display;
        OpponentName = snapshot.OpponentName;
        OpponentGoals = snapshot.OpponentScore.Goals;
        OpponentPoints = snapshot.OpponentScore.Points;
        OpponentTotal = snapshot.OpponentScore.Total;
        OpponentScoreDisplay = snapshot.OpponentScore.Display;
        Result = snapshot.Result;
        Summary = snapshot.Summary;
        LiveUrl = snapshot.LiveUrl?.OriginalString;
        OriginalJson = originalJson;

        Raise(new MatchCompletedDomainEvent(Guid.NewGuid(), Id, snapshot));
    }

    public Guid Id { get; private set; }

    public string ProviderEventId { get; private set; }

    public string ProviderMatchId { get; private set; }

    public string EventType { get; private set; }

    public string ApiVersion { get; private set; }

    public DateTime ProviderCreatedAtUtc { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }

    public string Status { get; private set; }

    public string Sport { get; private set; }

    public string? Competition { get; private set; }

    public string? Stage { get; private set; }

    public DateTime EndedAtUtc { get; private set; }

    public string? VenueName { get; private set; }

    public string? VenueType { get; private set; }

    public string TeamName { get; private set; }

    public int TeamGoals { get; private set; }

    public int TeamPoints { get; private set; }

    public int TeamTotal { get; private set; }

    public string TeamScoreDisplay { get; private set; }

    public string OpponentName { get; private set; }

    public int OpponentGoals { get; private set; }

    public int OpponentPoints { get; private set; }

    public int OpponentTotal { get; private set; }

    public string OpponentScoreDisplay { get; private set; }

    public string Result { get; private set; }

    public string? Summary { get; private set; }

    public string? LiveUrl { get; private set; }

    public string OriginalJson { get; private set; }

    public static MatchCompletion Create(MatchCompletionSnapshot snapshot, string originalJson) =>
        new(snapshot, originalJson);
}
