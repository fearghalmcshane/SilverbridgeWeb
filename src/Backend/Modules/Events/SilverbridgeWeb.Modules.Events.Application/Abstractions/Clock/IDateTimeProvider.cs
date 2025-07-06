namespace SilverbridgeWeb.Modules.Events.Application.Abstractions.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
