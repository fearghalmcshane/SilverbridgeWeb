namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public sealed record PaymentRequestLineDetails(string Description, int Quantity, long UnitAmountMinor);
