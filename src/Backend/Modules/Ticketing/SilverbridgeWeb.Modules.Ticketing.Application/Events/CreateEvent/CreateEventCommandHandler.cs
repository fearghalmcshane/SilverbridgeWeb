using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Ticketing.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Ticketing.Domain.Events;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Events.CreateEvent;

internal sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    ITicketTypeRepository ticketTypeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEventCommand>
{
    public async Task<Result> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = Event.Create(
            request.EventId,
            request.Title,
            request.Description,
            request.Location,
            request.StartsAtUtc,
            request.EndsAtUtc);

        eventRepository.Insert(@event);

        IEnumerable<TicketType> ticketTypes = request.TicketTypes
            .Select(t => TicketType.Create(t.TicketTypeId, t.EventId, t.Name, t.Price, t.Currency, t.Quantity));

        ticketTypeRepository.InsertRange(ticketTypes);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
