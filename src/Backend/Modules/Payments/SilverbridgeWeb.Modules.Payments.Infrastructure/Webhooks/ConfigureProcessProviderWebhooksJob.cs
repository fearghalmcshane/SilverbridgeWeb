using Microsoft.Extensions.Options;
using Quartz;

namespace SilverbridgeWeb.Modules.Payments.Infrastructure.Webhooks;

internal sealed class ConfigureProcessProviderWebhooksJob(IOptions<PaymentsOptions> options)
    : IConfigureOptions<QuartzOptions>
{
    private readonly WebhookProcessingOptions _webhookOptions = options.Value.Webhooks;

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(ProcessProviderWebhooksJob).FullName!;

        options
            .AddJob<ProcessProviderWebhooksJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(_webhookOptions.IntervalInSeconds).RepeatForever()));
    }
}
