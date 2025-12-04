using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Attendance.Application.Events;

public sealed record CreateEventCommand(
    Guid EventId,
    string Title,
    string Description,
    string Location,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc) : ICommand;
