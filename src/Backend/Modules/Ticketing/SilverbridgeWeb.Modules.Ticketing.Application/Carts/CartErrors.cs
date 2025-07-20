using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Ticketing.Application.Carts;

public static class CartErrors
{
    public static readonly Error Empty = Error.Problem("Carts.Empty", "The cart is empty");
}
