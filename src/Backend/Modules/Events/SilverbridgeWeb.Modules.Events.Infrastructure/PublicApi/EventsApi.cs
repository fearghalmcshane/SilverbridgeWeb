using MediatR;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Events.Application.TicketTypes.GetTicketType;
using SilverbridgeWeb.Modules.Events.PublicApi;
using TicketTypeResponse = SilverbridgeWeb.Modules.Events.PublicApi.TicketTypeResponse;

namespace SilverbridgeWeb.Modules.Events.Infrastructure.PublicApi;

internal sealed class EventsApi(ISender sender) : IEventsApi
{
    public async Task<TicketTypeResponse?> GetTicketTypeAsync(
        Guid ticketTypeId,
        CancellationToken cancellationToken = default)
    {
        Result<Application.TicketTypes.GetTicketType.TicketTypeResponse> result =
            await sender.Send(new GetTicketTypeQuery(ticketTypeId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        return new TicketTypeResponse(
            result.Value.Id,
            result.Value.EventId,
            result.Value.Name,
            result.Value.Price,
            result.Value.Currency,
            result.Value.Quantity);
    }
}
