using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Events.CancelEvent;

public sealed record CancelEventCommand(Guid EventId) : ICommand;
