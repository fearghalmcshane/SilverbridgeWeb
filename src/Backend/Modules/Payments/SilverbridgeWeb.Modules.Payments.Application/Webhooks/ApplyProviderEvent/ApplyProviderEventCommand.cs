using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

namespace SilverbridgeWeb.Modules.Payments.Application.Webhooks.ApplyProviderEvent;

public sealed record ApplyProviderEventCommand(ProviderEvent Event) : ICommand;
