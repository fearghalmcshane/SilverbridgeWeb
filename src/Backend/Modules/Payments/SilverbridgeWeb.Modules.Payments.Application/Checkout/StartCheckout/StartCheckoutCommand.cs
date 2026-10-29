using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Checkout.StartCheckout;

public sealed record StartCheckoutCommand(Guid PaymentRequestId, Guid UserId) : ICommand<CheckoutRedirectResponse>;

public sealed record CheckoutRedirectResponse(Uri RedirectUrl);
