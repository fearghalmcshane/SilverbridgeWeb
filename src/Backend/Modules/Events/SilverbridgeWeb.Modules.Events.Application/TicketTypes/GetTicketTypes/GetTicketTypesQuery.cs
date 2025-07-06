using SilverbridgeWeb.Modules.Events.Application.Abstractions.Messaging;
using SilverbridgeWeb.Modules.Events.Application.TicketTypes.GetTicketType;

namespace SilverbridgeWeb.Modules.Events.Application.TicketTypes.GetTicketTypes;

public sealed record GetTicketTypesQuery(Guid EventId) : IQuery<IReadOnlyCollection<TicketTypeResponse>>;
