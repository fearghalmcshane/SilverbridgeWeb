using Microsoft.Extensions.Options;
using Quartz;

namespace SilverbridgeWeb.Modules.Matches.Infrastructure.Outbox;

internal sealed class ConfigureProcessOutboxJob(IOptions<MatchesOutboxOptions> outboxOptions)
    : IConfigureOptions<QuartzOptions>
{
    private readonly MatchesOutboxOptions _options = outboxOptions.Value;

    public void Configure(QuartzOptions options)
    {
        string identity = typeof(ProcessOutboxJob).FullName!;
        options.AddJob<ProcessOutboxJob>(job => job.WithIdentity(identity))
            .AddTrigger(trigger => trigger.ForJob(identity)
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInSeconds(_options.IntervalInSeconds).RepeatForever()));
    }
}
