using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Payments.Application.Reconciliation.ReconcilePendingPayments;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Reconciliation;

[DisallowConcurrentExecution]
internal sealed class ReconcilePendingPaymentsJob(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<PaymentsOptions> options,
    ILogger<ReconcilePendingPaymentsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        Result<int> result = await sender.Send(
            new ReconcilePendingPaymentsCommand(options.Value.Reconciliation.BatchSize),
            context.CancellationToken);

        if (result.IsSuccess && result.Value > 0 && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Payments - Reconciled {Count} unsettled payment requests", result.Value);
        }
    }
}
