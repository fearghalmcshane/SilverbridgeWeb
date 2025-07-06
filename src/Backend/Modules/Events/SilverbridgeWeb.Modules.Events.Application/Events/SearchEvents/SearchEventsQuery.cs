using SilverbridgeWeb.Modules.Events.Application.Abstractions.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Events.SearchEvents;

public sealed record SearchEventsQuery(
    Guid? CategoryId,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page,
    int PageSize) : IQuery<SearchEventsResponse>;
