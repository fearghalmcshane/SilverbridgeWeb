namespace SilverbridgeWeb.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // Consider making public types internal
public abstract class BaseTest
#pragma warning restore CA1515 // Consider making public types internal
{
    protected const string UsersNamespace = "SilverbridgeWeb.Modules.Users";
    protected const string UsersIntegrationEventsNamespace = "SilverbridgeWeb.Modules.Users.IntegrationEvents";

    protected const string EventsNamespace = "SilverbridgeWeb.Modules.Events";
    protected const string EventsIntegrationEventsNamespace = "SilverbridgeWeb.Modules.Events.IntegrationEvents";

    protected const string TicketingNamespace = "SilverbridgeWeb.Modules.Ticketing";
    protected const string TicketingIntegrationEventsNamespace = "SilverbridgeWeb.Modules.Ticketing.IntegrationEvents";

    protected const string AttendanceNamespace = "SilverbridgeWeb.Modules.Attendance";
    protected const string AttendanceIntegrationEventsNamespace = "SilverbridgeWeb.Modules.Attendance.IntegrationEvents";
}
