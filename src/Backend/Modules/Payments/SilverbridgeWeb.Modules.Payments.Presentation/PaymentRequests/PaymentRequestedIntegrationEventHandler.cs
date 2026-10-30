using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CreatePaymentRequest;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Presentation.PaymentRequests;

internal sealed class PaymentRequestedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PaymentRequestedIntegrationEvent>
{
    public override async Task Handle(
        PaymentRequestedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new CreatePaymentRequestCommand(
                integrationEvent.PaymentRequestId,
                integrationEvent.Source,
                integrationEvent.SourceReference,
                integrationEvent.PayerUserId,
                integrationEvent.Description,
                integrationEvent.Currency,
                integrationEvent.Kind switch
                {
                    PaymentKind.MonthlySubscription => PaymentRequestKind.MonthlySubscription,
                    _ => PaymentRequestKind.OneOff
                },
                integrationEvent.Lines
                    .Select(l => new PaymentRequestLineDetails(l.Description, l.Quantity, l.UnitAmountMinor))
                    .ToList()),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(CreatePaymentRequestCommand), result.Error);
        }
    }
}
