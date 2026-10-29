using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

namespace SilverbridgeWeb.Modules.Payments.Application.PaymentRequests.CreatePaymentRequest;

internal sealed class CreatePaymentRequestCommandHandler(
    IPaymentRequestRepository paymentRequestRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreatePaymentRequestCommand>
{
    public async Task<Result> Handle(CreatePaymentRequestCommand request, CancellationToken cancellationToken)
    {
        // The requesting module owns the identifier, so a redelivered request is simply already here.
        if (await paymentRequestRepository.GetAsync(request.PaymentRequestId, cancellationToken) is not null)
        {
            return Result.Success();
        }

        Result<PaymentRequest> result = PaymentRequest.Create(
            request.PaymentRequestId,
            request.PayerUserId,
            request.Source,
            request.SourceReference,
            request.Description,
            request.Currency,
            request.Kind,
            request.Lines,
            dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        paymentRequestRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
