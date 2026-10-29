using Microsoft.Extensions.Logging;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CancelPaymentRequest;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Presentation.PaymentRequests;

internal sealed class PaymentRequestCancelledIntegrationEventHandler(
    ISender sender,
    ILogger<PaymentRequestCancelledIntegrationEventHandler> logger)
    : IntegrationEventHandler<PaymentRequestCancelledIntegrationEvent>
{
    public override async Task Handle(
        PaymentRequestCancelledIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new CancelPaymentRequestCommand(integrationEvent.PaymentRequestId),
            cancellationToken);

        // A payment that already settled cannot be cancelled; the requester hears the outcome through its own event.
        if (result.IsFailure && result.Error.Type == ErrorType.Conflict)
        {
            logger.LogWarning(
                "Ignored cancellation of payment request {PaymentRequestId}: {Reason}",
                integrationEvent.PaymentRequestId,
                result.Error.Description);
            return;
        }

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(CancelPaymentRequestCommand), result.Error);
        }
    }
}
