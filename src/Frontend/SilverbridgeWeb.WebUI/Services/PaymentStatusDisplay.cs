using MudBlazor;
using SilverbridgeWeb.WebUI.Services.ApiClients;

namespace SilverbridgeWeb.WebUI.Services;

/// <summary>How a payment status is shown to people.</summary>
internal static class PaymentStatusDisplay
{
    public static string LabelFor(PaymentStatus status, bool partiallyRefunded = false) =>
        status switch
        {
            PaymentStatus.Pending or PaymentStatus.AwaitingPayment => "Awaiting payment",
            PaymentStatus.Processing => "Processing",
            PaymentStatus.Succeeded when partiallyRefunded => "Partly refunded",
            PaymentStatus.Succeeded => "Paid",
            PaymentStatus.Failed => "Failed",
            PaymentStatus.Expired => "Expired",
            PaymentStatus.Cancelled => "Cancelled",
            PaymentStatus.Refunded => "Refunded",
            _ => status.ToString()
        };

    public static Color ColorFor(PaymentStatus status) =>
        status switch
        {
            PaymentStatus.Pending or PaymentStatus.AwaitingPayment => Color.Warning,
            PaymentStatus.Processing => Color.Info,
            PaymentStatus.Succeeded => Color.Success,
            PaymentStatus.Failed => Color.Error,
            PaymentStatus.Refunded => Color.Secondary,
            _ => Color.Default
        };
}
