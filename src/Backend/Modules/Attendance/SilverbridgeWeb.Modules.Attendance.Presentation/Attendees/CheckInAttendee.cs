using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Common.Presentation.Results;
using SilverbridgeWeb.Common.Presentation.Endpoints;
using SilverbridgeWeb.Modules.Attendance.Application.Abstractions.Authentication;
using SilverbridgeWeb.Modules.Attendance.Application.Attendees.CheckInAttendee;

namespace SilverbridgeWeb.Modules.Attendance.Presentation.Attendees;

internal sealed class CheckInAttendee : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("attendees/check-in", async (
                Request request,
                IAttendanceContext attendanceContext,
                ISender sender) =>
        {
            Result result = await sender.Send(
                new CheckInAttendeeCommand(attendanceContext.AttendeeId, request.TicketId));

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.CheckInTicket)
        .WithTags(Tags.Attendees);
    }

    internal sealed class Request
    {
        public Guid TicketId { get; init; }
    }
}
