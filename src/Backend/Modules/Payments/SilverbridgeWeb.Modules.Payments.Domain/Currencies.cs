namespace SilverbridgeWeb.Modules.Payments.Domain;

public static class Currencies
{
    public const string Gbp = "GBP";

    public static bool IsSupported(string? currency) => currency == Gbp;
}
