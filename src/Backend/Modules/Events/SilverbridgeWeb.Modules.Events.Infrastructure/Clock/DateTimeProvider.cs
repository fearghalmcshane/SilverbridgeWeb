using SilverbridgeWeb.Modules.Events.Application.Abstractions.Clock;

namespace SilverbridgeWeb.Modules.Events.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
