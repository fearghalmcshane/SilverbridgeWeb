namespace SilverbridgeWeb.Modules.Payments.IntegrationEvents;

public sealed record PaymentLineModel(string Description, int Quantity, long UnitAmountMinor);
