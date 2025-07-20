using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Tickets.CreateTicketBatch;

public sealed record CreateTicketBatchCommand(Guid OrderId) : ICommand;
