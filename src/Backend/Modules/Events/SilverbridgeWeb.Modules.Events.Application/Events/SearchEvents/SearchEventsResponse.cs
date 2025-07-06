using SilverbridgeWeb.Modules.Events.Application.Events.GetEvents;

namespace SilverbridgeWeb.Modules.Events.Application.Events.SearchEvents;

public sealed record SearchEventsResponse(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<EventResponse> Events);
