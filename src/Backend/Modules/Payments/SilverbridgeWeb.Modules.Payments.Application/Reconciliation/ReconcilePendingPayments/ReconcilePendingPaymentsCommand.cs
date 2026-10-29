using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Payments.Application.Reconciliation.ReconcilePendingPayments;

/// <returns>How many payment requests were checked with the provider.</returns>
public sealed record ReconcilePendingPaymentsCommand(int BatchSize) : ICommand<int>;
