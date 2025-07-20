using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Payments.RefundPaymentsForEvent;

public sealed record RefundPaymentsForEventCommand(Guid EventId) : ICommand;
