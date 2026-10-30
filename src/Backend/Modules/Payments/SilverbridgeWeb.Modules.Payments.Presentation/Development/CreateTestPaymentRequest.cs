using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SilverbridgeWeb.Common.Application.Clock;
using SilverbridgeWeb.Common.Application.EventBus;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Domain;
using SilverbridgeWeb.Modules.Payments.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Payments.Presentation.Development;

/// <summary>
/// Development only: asks for a payment from the signed-in user the same way a real module would, so the whole
/// flow can be tried before any module requests payments. Never mapped outside Development.
/// </summary>
internal sealed class CreateTestPaymentRequest : IEndpoint
{
    internal const string Source = "development";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        if (!app.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return;
        }

        app.MapPost("payments/dev/requests", async (
            Request request,
            IPaymentsUserContext userContext,
            IEventBus eventBus,
            IDateTimeProvider dateTimeProvider,
            CancellationToken cancellationToken) =>
        {
            var paymentRequestId = Ulid.NewUlid().ToGuid();

            await eventBus.PublishAsync(
                new PaymentRequestedIntegrationEvent(
                    Guid.NewGuid(),
                    dateTimeProvider.UtcNow,
                    paymentRequestId,
                    Source,
                    $"test-{paymentRequestId:N}",
                    userContext.UserId,
                    request.Description,
                    Currencies.Gbp,
                    request.Kind,
                    [new PaymentLineModel(request.Description, 1, request.AmountMinor)]),
                cancellationToken);

            return Results.Ok(new { PaymentRequestId = paymentRequestId, PayUrl = $"/payments/{paymentRequestId}" });
        })
        .RequireAuthorization()
        .WithTags(Tags.Payments);
    }

    internal sealed record Request(string Description, long AmountMinor, PaymentKind Kind = PaymentKind.OneOff);
}
