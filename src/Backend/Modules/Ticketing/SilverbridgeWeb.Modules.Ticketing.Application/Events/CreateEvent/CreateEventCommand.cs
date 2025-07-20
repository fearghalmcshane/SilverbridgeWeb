using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    Guid EventId,
    string Title,
    string Description,
    string Location,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc,
    List<CreateEventCommand.TicketTypeRequest> TicketTypes) : ICommand
{
    public sealed record TicketTypeRequest(
        Guid TicketTypeId,
        Guid EventId,
        string Name,
        decimal Price,
        string Currency,
        decimal Quantity);
}
