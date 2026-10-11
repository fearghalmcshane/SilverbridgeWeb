using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Matches.Domain.Matches;
using SilverbridgeWeb.Modules.Matches.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Matches.Application.Matches.CaptureMatchCompletion;

internal sealed class MatchCompletedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<MatchCompletedDomainEvent>
{
    public override Task Handle(MatchCompletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        MatchCompletionSnapshot source = domainEvent.Snapshot;
        var snapshot = new MatchCompletedSnapshot(
            source.ProviderEventId,
            source.ProviderMatchId,
            source.EventType,
            source.ApiVersion,
            source.ProviderCreatedAtUtc,
            source.ReceivedAtUtc,
            source.Status,
            source.Sport,
            source.Competition,
            source.Stage,
            source.EndedAtUtc,
            source.VenueName,
            source.VenueType,
            source.TeamName,
            new MatchScore(source.TeamScore.Goals, source.TeamScore.Points, source.TeamScore.Total, source.TeamScore.Display),
            source.OpponentName,
            new MatchScore(
                source.OpponentScore.Goals,
                source.OpponentScore.Points,
                source.OpponentScore.Total,
                source.OpponentScore.Display),
            source.Result,
            source.Summary,
            source.LiveUrl);

        return eventBus.PublishAsync(
            new MatchCompletedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.CompletionId,
                snapshot),
            cancellationToken);
    }
}
