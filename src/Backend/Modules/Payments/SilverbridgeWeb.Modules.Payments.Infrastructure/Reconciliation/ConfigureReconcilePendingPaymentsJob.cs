using Microsoft.Extensions.Options;
using Quartz;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Reconciliation;

internal sealed class ConfigureReconcilePendingPaymentsJob(IOptions<PaymentsOptions> options)
    : IConfigureOptions<QuartzOptions>
{
    private readonly ReconciliationOptions _reconciliationOptions = options.Value.Reconciliation;

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(ReconcilePendingPaymentsJob).FullName!;

        options
            .AddJob<ReconcilePendingPaymentsJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInMinutes(_reconciliationOptions.IntervalInMinutes).RepeatForever()));
    }
}
