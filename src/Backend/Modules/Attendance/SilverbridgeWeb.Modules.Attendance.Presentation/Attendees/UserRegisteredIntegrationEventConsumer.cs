using MassTransit;
using MediatR;
using SilverbridgeWeb.Common.Application.Exceptions;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Attendance.Application.Attendees.CreateAttendee;
using SilverbridgeWeb.Modules.Users.IntegrationEvents;

namespace SilverbridgeWeb.Modules.Attendance.Presentation.Attendees;

public sealed class UserRegisteredIntegrationEventConsumer(ISender sender)
    : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        Result result = await sender.Send(
            new CreateAttendeeCommand(
                context.Message.UserId,
                context.Message.Email,
                context.Message.FirstName,
                context.Message.LastName),
            context.CancellationToken);

        if (result.IsFailure)
        {
            throw new SilverbridgeWebException(nameof(CreateAttendeeCommand), result.Error);
        }
    }
}
