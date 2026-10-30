using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Payments;
using SilverbridgeWeb.Modules.Payments.Domain.Subscriptions;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CancelSubscription;

internal sealed class CancelSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IPaymentProvider paymentProvider,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CancelSubscriptionCommand>
{
    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        Subscription? subscription = await subscriptionRepository.GetAsync(request.SubscriptionId, cancellationToken);
        if (subscription is null ||
            request.RequestingUserId is not null && subscription.PayerUserId != request.RequestingUserId)
        {
            return Result.Failure(SubscriptionErrors.NotFound(request.SubscriptionId));
        }

        if (subscription.Status == SubscriptionStatus.Cancelled)
        {
            return Result.Success();
        }

        Result providerResult = await paymentProvider.CancelSubscriptionAsync(
            subscription.ProviderSubscriptionId,
            cancellationToken);
        if (providerResult.IsFailure)
        {
            return providerResult;
        }

        // The provider confirms with a webhook too; recording it now keeps the page accurate straight away.
        DateTime utcNow = dateTimeProvider.UtcNow;
        subscription.UpdateStatus(SubscriptionStatus.Cancelled, null, utcNow, utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
