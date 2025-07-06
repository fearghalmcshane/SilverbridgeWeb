using SilverbridgeWeb.Modules.Events.Application.Abstractions.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Events.RescheduleEvent;

public sealed record RescheduleEventCommand(Guid EventId, DateTime StartsAtUtc, DateTime? EndsAtUtc) : ICommand;
