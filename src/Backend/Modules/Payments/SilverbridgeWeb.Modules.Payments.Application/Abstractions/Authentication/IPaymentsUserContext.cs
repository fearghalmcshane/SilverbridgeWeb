namespace SilverbridgeWeb.Modules.Payments.Application.Abstractions.Authentication;

public interface IPaymentsUserContext
{
    Guid UserId { get; }

    bool HasPermission(string permission);
}
