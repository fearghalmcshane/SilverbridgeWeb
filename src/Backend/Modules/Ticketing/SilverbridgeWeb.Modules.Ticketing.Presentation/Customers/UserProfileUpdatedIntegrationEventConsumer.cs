using MassTransit;
using MediatR;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Ticketing.Application.Customers.UpdateCustomer;
using SilverbridgeWeb.Modules.Users.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Ticketing.Presentation.Customers;

public sealed class UserProfileUpdatedIntegrationEventConsumer(ISender sender)
    : IConsumer<UserProfileUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserProfileUpdatedIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new UpdateCustomerCommand(
                context.Message.UserId,
                context.Message.FirstName,
                context.Message.LastName),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(UpdateCustomerCommand), result.Error);
        }
    }
}
