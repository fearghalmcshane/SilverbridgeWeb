using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Attendance.Application.Abstractions.Data;
using SilverbridgeWeb.Modules.Attendance.Domain.Attendees;

namespace SilverbridgeWeb.Modules.Attendance.Application.Attendees.CreateAttendee;

internal sealed class CreateAttendeeCommandHandler(IAttendeeRepository attendeeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAttendeeCommand>
{
    public async Task<Result> Handle(CreateAttendeeCommand request, CancellationToken cancellationToken)
    {
        var attendee = Attendee.Create(request.AttendeeId, request.Email, request.FirstName, request.LastName);

        attendeeRepository.Insert(attendee);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
