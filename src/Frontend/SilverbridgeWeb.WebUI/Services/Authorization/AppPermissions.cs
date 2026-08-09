namespace SilverbridgeWeb.WebUI.Services.Authorization;

internal static class AppPermissions
{
    public const string ViewBookings = "bookings:read";
    public const string AddFacilities = "bookings:facilities:create";
    public const string CreateBookings = "bookings:create";
    public const string ApproveBookings = "bookings:approve";
    public const string UpdateBookings = "bookings:update";
    public const string DeleteBookings = "bookings:delete";
}
