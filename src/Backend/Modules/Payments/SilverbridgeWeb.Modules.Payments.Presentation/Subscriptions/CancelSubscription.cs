using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CancelSubscription;

namespace SilverbridgeWeb.Modules.Payments.Presentation.Subscriptions;

internal sealed class CancelSubscription : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("payments/subscriptions/{id:guid}/cancel", async (
            Guid id,
            IPaymentsUserContext userContext,
            ISender sender) =>
        {
            // Payers may cancel their own subscriptions; managers may cancel anyone's.
            Guid? requestingUserId = userContext.HasPermission(Permissions.ManagePayments) ? null : userContext.UserId;

            Result result = await sender.Send(new CancelSubscriptionCommand(id, requestingUserId));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Subscriptions);
    }
}
