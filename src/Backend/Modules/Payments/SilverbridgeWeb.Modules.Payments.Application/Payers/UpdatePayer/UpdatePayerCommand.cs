using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Payers.UpdatePayer;

public sealed record UpdatePayerCommand(Guid UserId, string FirstName, string LastName) : ICommand;
