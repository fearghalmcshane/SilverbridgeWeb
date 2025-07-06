using SilverbridgeWeb.Modules.Events.Application.Abstractions.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Events.PublishEvent;

public sealed record PublishEventCommand(Guid EventId) : ICommand;
