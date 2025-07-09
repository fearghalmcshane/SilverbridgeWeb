using SilverbridgeWeb.Common.Application.Clock;

namespace SilverbridgeWeb.Common.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
