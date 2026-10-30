using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Webhooks.ReceiveProviderWebhook;

public sealed record ReceiveProviderWebhookCommand(string Payload, string? Signature) : ICommand;
