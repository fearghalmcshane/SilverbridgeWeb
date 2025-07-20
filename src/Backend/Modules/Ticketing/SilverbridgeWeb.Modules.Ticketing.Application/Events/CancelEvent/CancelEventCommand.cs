using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Events.CancelEvent;

public sealed record CancelEventCommand(Guid EventId) : ICommand;
