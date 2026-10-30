using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;

namespace SilverbridgeWeb.Modules.Payments.Application.Subscriptions.CreateBillingPortalSession;

public sealed record CreateBillingPortalSessionCommand(Guid UserId) : ICommand<CheckoutRedirectResponse>;
