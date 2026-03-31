using System.Text.Json.Serialization;

namespace SilverbridgeWeb.Modules.Foireann.Application.DTOs;

public sealed record FoireannPagedResponse<T>(
    [property: JsonPropertyName("data")] IReadOnlyList<T> Data,
    [property: JsonPropertyName("links")] FoireannPaginationLinks Links,
    [property: JsonPropertyName("page")] FoireannPageInfo Page);

public sealed record FoireannPageInfo(
    [property: JsonPropertyName("page")] long Page,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("totalPages")] long TotalPages,
    [property: JsonPropertyName("totalElements")] long TotalElements);

public sealed record FoireannPaginationLinks(
    [property: JsonPropertyName("self")] FoireannLinkRef? Self,
    [property: JsonPropertyName("first")] FoireannLinkRef? First,
    [property: JsonPropertyName("next")] FoireannLinkRef? Next,
    [property: JsonPropertyName("prev")] FoireannLinkRef? Prev,
    [property: JsonPropertyName("last")] FoireannLinkRef? Last);

public sealed record FoireannLinkRef(
    [property: JsonPropertyName("href")] string? Href);
