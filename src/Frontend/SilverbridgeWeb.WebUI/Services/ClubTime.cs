namespace SilverbridgeWeb.WebUI.Services;

/// <summary>
/// Converts between UTC and club-local time for display.
/// </summary>
/// <remarks>
/// Rendering happens server-side (InteractiveServer), and the Azure container runs in UTC, so
/// <c>ToLocalTime()</c>/<c>LocalDateTime</c> silently produce UTC in production. The zone must be explicit.
/// </remarks>
internal static class ClubTime
{
    public static TimeZoneInfo TimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById("Europe/Dublin");

    public static DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZone);

    public static DateTime ToLocal(DateTimeOffset value) =>
        TimeZoneInfo.ConvertTime(value, TimeZone).DateTime;

    public static DateTime ToUtc(DateTime clubLocal) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(clubLocal, DateTimeKind.Unspecified), TimeZone);

    public static DateTime Today => ToLocal(DateTime.UtcNow).Date;

    public static string ToUtcIso(DateTime clubLocal) =>
        ToUtc(clubLocal).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture);
}
