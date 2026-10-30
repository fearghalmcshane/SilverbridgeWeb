using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Payers.CreatePayer;

public sealed record CreatePayerCommand(Guid UserId, string Email, string FirstName, string LastName) : ICommand;
