using MediatR;
using SilverbridgeWeb.Modules.Events.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Events.Domain.Events;

namespace SilverbridgeWeb.Modules.Events.Application.Events.CreateEvent;

internal sealed class CreateEventCommandHandler(IEventRepository eventRepository, IUnitOfwork unitOfwork) : IRequestHandler<CreateEventCommand, Guid>
{
    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = Event.Create(
            request.Title,
            request.Description,
            request.Location,
            request.StartsAtUtc,
            request.EndsAtUtc);

        eventRepository.Insert(@event);

        await unitOfwork.SaveChangesAsync(cancellationToken);

        return @event.Id;
    }
}
