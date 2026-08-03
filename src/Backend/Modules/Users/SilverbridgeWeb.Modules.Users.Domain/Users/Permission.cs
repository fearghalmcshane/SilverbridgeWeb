namespace SilverbridgeWeb.Modules.Users.Domain.Users;

public sealed class Permission
{
    public static readonly Permission GetUser = new("users:read");
    public static readonly Permission ModifyUser = new("users:update");
    public static readonly Permission ModifyRoles = new("users:roles:update");
    public static readonly Permission GetEvents = new("events:read");
    public static readonly Permission SearchEvents = new("events:search");
    public static readonly Permission ModifyEvents = new("events:update");
    public static readonly Permission GetArticles = new("news:read");
    public static readonly Permission SearchArticles = new("news:search");
    public static readonly Permission CreateArticles = new("news:create");
    public static readonly Permission UpdateArticles = new("news:update");
    public static readonly Permission PublishArticles = new("news:publish");
    public static readonly Permission DeleteArticles = new("news:delete");
    public static readonly Permission UploadArticleMedia = new("news:media:upload");
    public static readonly Permission ModifyNewsCategories = new("news:categories:update");
    public static readonly Permission GetTicketTypes = new("ticket-types:read");
    public static readonly Permission ModifyTicketTypes = new("ticket-types:update");
    public static readonly Permission GetCategories = new("categories:read");
    public static readonly Permission ModifyCategories = new("categories:update");
    public static readonly Permission GetCart = new("carts:read");
    public static readonly Permission AddToCart = new("carts:add");
    public static readonly Permission RemoveFromCart = new("carts:remove");
    public static readonly Permission GetOrders = new("orders:read");
    public static readonly Permission CreateOrder = new("orders:create");
    public static readonly Permission GetTickets = new("tickets:read");
    public static readonly Permission CheckInTicket = new("tickets:check-in");
    public static readonly Permission GetEventStatistics = new("event-statistics:read");

    public Permission(string code)
    {
        Code = code;
    }

    public string Code { get; }
}
