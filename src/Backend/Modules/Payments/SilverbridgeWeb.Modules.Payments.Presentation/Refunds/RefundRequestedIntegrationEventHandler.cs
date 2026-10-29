using Microsoft.Extensions.Logging;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Presentation.Refunds;

internal sealed class RefundRequestedIntegrationEventHandler(
    ISender sender,
    ILogger<RefundRequestedIntegrationEventHandler> logger)
    : IntegrationEventHandler<RefundRequestedIntegrationEvent>
{
    public override async Task Handle(
        RefundRequestedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result<Guid> result = await sender.Send(
            new RefundPaymentCommand(
                integrationEvent.PaymentRequestId,
                integrationEvent.AmountMinor,
                integrationEvent.Reason,
                RequestedByUserId: null),
            cancellationToken);

        // Nothing left to refund, or nothing was ever paid: there is no refund to make and retrying will not change that.
        if (result.IsFailure && result.Error.Type is ErrorType.Conflict or ErrorType.Problem)
        {
            logger.LogWarning(
                "Ignored refund request for payment request {PaymentRequestId}: {Reason}",
                integrationEvent.PaymentRequestId,
                result.Error.Description);
            return;
        }

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(RefundPaymentCommand), result.Error);
        }
    }
}
