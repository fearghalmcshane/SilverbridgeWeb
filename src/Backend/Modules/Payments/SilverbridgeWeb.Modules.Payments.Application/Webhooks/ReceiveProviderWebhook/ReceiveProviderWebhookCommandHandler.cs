using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;

namespace SilverbridgeWeb.Modules.Payments.Application.Webhooks.ReceiveProviderWebhook;

/// <summary>
/// Verifies a provider callback and stores it for the background job, so the provider gets a quick answer
/// and a slow or failing database update never makes it give up on the event.
/// </summary>
internal sealed class ReceiveProviderWebhookCommandHandler(
    IProviderWebhookVerifier webhookVerifier,
    IProviderWebhookInbox webhookInbox)
    : ICommandHandler<ReceiveProviderWebhookCommand>
{
    public async Task<Result> Handle(ReceiveProviderWebhookCommand request, CancellationToken cancellationToken)
    {
        Result<VerifiedProviderWebhook> verification = webhookVerifier.Verify(request.Payload, request.Signature);
        if (verification.IsFailure)
        {
            return verification;
        }

        await webhookInbox.AddAsync(verification.Value, request.Payload, cancellationToken);

        return Result.Success();
    }
}
