﻿using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Ticketing.Domain.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid paymentId) =>
        Error.NotFound("Payments.NotFound", $"The payment with the identifier {paymentId} was not found");

    public static readonly Error AlreadyRefunded =
        Error.Problem("Payments.AlreadyRefunded", "The payment was already refunded");

    public static readonly Error NotEnoughFunds =
        Error.Problem("Payments.NotEnoughFunds", "There are not enough funds for a refund");
}
