using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.Refunds.RefundPayment;

/// <summary>
/// Asks the provider for a refund before saving anything. The refund identifier comes from the caller and is the
/// provider's idempotency key, so a retry after a lost response or a failed save gets the same refund back rather
/// than a second one. It is also in the provider's metadata, so the refund webhook records it under that identifier
/// even if this handler never saves; a retry after that finds it already recorded.
/// </summary>
internal sealed class RefundPaymentCommandHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IPaymentProvider paymentProvider,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RefundPaymentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        PaymentRequest? paymentRequest = await paymentRequestRepository.GetAsync(request.PaymentRequestId, cancellationToken);
        if (paymentRequest is null)
        {
            return Result.Failure<Guid>(PaymentRequestErrors.NotFound(request.PaymentRequestId));
        }

        // Already sent to the provider and saved by an earlier attempt or by its webhook.
        if (paymentRequest.Refunds.Any(r => r.Id == request.RefundId))
        {
            return request.RefundId;
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        Result<Refund> refundResult = paymentRequest.RequestRefund(
            request.RefundId, request.AmountMinor, request.Reason, request.RequestedByUserId, utcNow);
        if (refundResult.IsFailure)
        {
            return Result.Failure<Guid>(refundResult.Error);
        }

        Refund refund = refundResult.Value;

        Result<ProviderRefund> providerResult = await paymentProvider.CreateRefundAsync(
            new ProviderRefundRequest(refund.Id, paymentRequest.Id, paymentRequest.ProviderPaymentIntentId!, refund.AmountMinor),
            cancellationToken);
        if (providerResult.IsFailure)
        {
            return Result.Failure<Guid>(providerResult.Error);
        }

        ProviderRefund providerRefund = providerResult.Value;

        Result applyResult = paymentRequest.ApplyRefundUpdate(
            providerRefund.ProviderRefundId,
            refund.Id,
            refund.AmountMinor,
            providerRefund.Status,
            providerRefund.FailureReason,
            utcNow);
        if (applyResult.IsFailure)
        {
            return Result.Failure<Guid>(applyResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return refund.Id;
    }
}
