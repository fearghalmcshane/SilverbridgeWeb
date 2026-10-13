using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Matches.IntegrationEvents;
using SilverbridgeWeb.Modules.News.Application.Articles.CreateMatchReportFromCompletion;
using SilverbridgeWeb.Modules.News.Domain.Articles;

namespace SilverbridgeWeb.Modules.News.Presentation.Matches;

internal sealed class MatchCompletedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<MatchCompletedIntegrationEvent>
{
    public override async Task Handle(
        MatchCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        MatchCompletedSnapshot snapshot = integrationEvent.Snapshot;

        Result<Guid?> result = await sender.Send(
            new CreateMatchReportFromCompletionCommand(
                MatchReportSources.TeamTally,
                snapshot.ProviderMatchId,
                snapshot.TeamName,
                snapshot.TeamScore.Goals,
                snapshot.TeamScore.Points,
                snapshot.TeamScore.Display,
                snapshot.OpponentName,
                snapshot.OpponentScore.Goals,
                snapshot.OpponentScore.Points,
                snapshot.OpponentScore.Display,
                snapshot.Result,
                snapshot.Competition,
                snapshot.Stage,
                snapshot.VenueName,
                snapshot.VenueType,
                snapshot.EndedAtUtc),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(CreateMatchReportFromCompletionCommand), result.Error);
        }
    }
}
