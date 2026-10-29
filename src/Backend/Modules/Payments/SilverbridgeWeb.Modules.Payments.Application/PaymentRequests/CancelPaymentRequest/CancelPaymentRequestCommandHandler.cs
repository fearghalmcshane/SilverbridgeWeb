using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CancelPaymentRequest;

internal sealed class CancelPaymentRequestCommandHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IPaymentProvider paymentProvider,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CancelPaymentRequestCommand>
{
    public async Task<Result> Handle(CancelPaymentRequestCommand request, CancellationToken cancellationToken)
    {
        PaymentRequest? paymentRequest = await paymentRequestRepository.GetAsync(request.PaymentRequestId, cancellationToken);
        if (paymentRequest is null)
        {
            return Result.Failure(PaymentRequestErrors.NotFound(request.PaymentRequestId));
        }

        // Close the provider's checkout page first so the payer cannot pay for something being cancelled.
        if (paymentRequest.Status == PaymentRequestStatus.AwaitingPayment &&
            paymentRequest.ProviderCheckoutSessionId is not null)
        {
            Result expireResult = await paymentProvider.ExpireCheckoutSessionAsync(
                paymentRequest.ProviderCheckoutSessionId,
                cancellationToken);
            if (expireResult.IsFailure)
            {
                return expireResult;
            }
        }

        Result result = paymentRequest.Cancel(dateTimeProvider.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
