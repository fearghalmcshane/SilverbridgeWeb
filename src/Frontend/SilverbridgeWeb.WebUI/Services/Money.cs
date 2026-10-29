using System.Globalization;

namespace SilverbridgeWeb.WebUI.Services;

/// <summary>Formats amounts held in minor units (pence) for display.</summary>
internal static class Money
{
    private static readonly CultureInfo s_gbCulture = CultureInfo.GetCultureInfo("en-GB");

    public static string Format(long amountMinor, string currency)
    {
        decimal amount = amountMinor / 100m;

        return string.Equals(currency, "GBP", StringComparison.OrdinalIgnoreCase)
            ? amount.ToString("C", s_gbCulture)
            : $"{amount.ToString("N2", CultureInfo.InvariantCulture)} {currency}";
    }

    /// <summary>Reads a pounds-and-pence amount such as <c>12.50</c> typed by a person into pence.</summary>
    public static bool TryParsePounds(string? input, out long amountMinor)
    {
        amountMinor = 0;
        string trimmed = (input ?? string.Empty).Trim().TrimStart('£');
        if (!decimal.TryParse(trimmed, NumberStyles.Number, s_gbCulture, out decimal pounds) ||
            pounds <= 0 ||
            decimal.Round(pounds, 2) != pounds)
        {
            return false;
        }

        amountMinor = (long)(pounds * 100);
        return true;
    }
}
