namespace SilverbridgeWeb.Modules.Payments.Domain.PaymentRequests;

public enum PaymentRequestStatus
{
    Pending = 0,
    AwaitingPayment = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Expired = 5,
    Cancelled = 6,
    Refunded = 7
}
